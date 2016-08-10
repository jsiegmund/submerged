#r "D:\Program Files (x86)\SiteExtensions\Functions\0.3.10261\bin\Microsoft.Azure.WebJobs.Extensions.NotificationHubs.dll" 

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions;

public static void Run(string inputEventMessage, string inputBlob, IBinder binder, out string outputBlob, TraceWriter log)
{
    if (String.IsNullOrEmpty(inputEventMessage))
    {
        log.Info($"The inputEventMessage array was null or didn't contain exactly one item.");
        outputBlob = inputBlob;
        
        return;
    }

    log.Info($"C# Event Hub trigger function processed a message: {inputEventMessage}"); 
     
    if (String.IsNullOrEmpty(inputBlob))
        inputBlob = DateTime.MinValue.ToString();
    
    DateTime lastEvent = DateTime.Parse(inputBlob);
    TimeSpan duration = DateTime.Now - lastEvent;
        
    if (duration.TotalMinutes >= 60) {
        ProcessNotifications(inputEventMessage, binder, log).Wait();
        outputBlob = DateTime.Now.ToString();
        
        log.Info("Notification sent, outputBlob updated with new datetime");
    } 
    else {
        log.Info($"Not sending notification message because of timer ({(int)duration.TotalMinutes} minutes ago).");
        outputBlob = inputBlob;
    }
}

private async static Task ProcessNotifications(string input, IBinder binder, TraceWriter log)
{
    string message;
    
    dynamic eventData = (JObject)JsonConvert.DeserializeObject(input);
    
    if (eventData.readingtype == "leakage")
        message = String.Format("Leakage detected! Sensor '{0}' has detected a possible leak.", eventData.sensorname);
    else if (eventData.readingtype == "EventsMissing")
        message = String.Format("Device {0} hasn't sent any data since {1}. Connection problems?", eventData.deviceid, eventData.sensorvalue);
    else
        message = String.Format("Sensor {0} is reading {1:0.0}, threshold ({2}) is {3:0.0}.", eventData.sensorname, eventData.sensorvalue, eventData.@operator, eventData.threshold);
        
    string devicetag = "deviceid:" + eventData.deviceid;
    message = $"{{'data':{{'message':'{message}'}} }}";
    var notification = new GcmNotification(message);
    
    var attribute = new NotificationHubAttribute
    {
        ConnectionStringSetting = "repsaj-submerged-notificationhub_NOTIFICATIONHUB",
        HubName = "repsaj-submerged-notificationhub",
        TagExpression = devicetag
    };
    
    IAsyncCollector<Notification> notifications = binder.Bind<IAsyncCollector<Notification>>(attribute);
    await notifications.AddAsync(notification);

    log.Info($"Notification sent with tag '{devicetag}': {message}");

}