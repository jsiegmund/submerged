using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.NotificationHubs;

public static void Run(RuleMessage inputEventMessage, string inputBlob, out Notification notification, out string outputBlob, TraceWriter log)
{
    log.Info($"Testing deviceid: {inputEventMessage.deviceid}");
    
    if (inputEventMessage == null)
    {
        log.Info($"The inputEventMessage array was null or didn't contain exactly one item.");
        
        notification = null;
        outputBlob = inputBlob;
        
        return;
    }

    log.Info($"C# Event Hub trigger function processed a message: {inputEventMessage}"); 
     
    if (String.IsNullOrEmpty(inputBlob))
        inputBlob = DateTime.MinValue.ToString();
    
    DateTime lastEvent = DateTime.Parse(inputBlob);
    TimeSpan duration = DateTime.Now - lastEvent;
    
    if (duration.TotalMinutes >= 0) {
        notification = GetGcmMessage(inputEventMessage);
        //notification = GetTemplateNotification(myEventHubMessage); 
        log.Info($"Sending notification message: {notification.Body}");
        outputBlob = DateTime.Now.ToString();
    } 
    else {
        log.Info($"Not sending notification message because of timer ({(int)duration.TotalMinutes} minutes ago).");
        
        notification = null;
        outputBlob = inputBlob;
    }
}

private static Notification GetGcmMessage(RuleMessage input)
{
    string message;
    
    if (input.readingtype == "leakage")
        message = String.Format("Leakage detected! Sensor {0} has detected a possible leak.", input.reading);
    else if (input.readingtype == "EventsMissing")
        message = String.Format("Device {0} hasn't sent any data since {1}. Connection problems?", input.deviceid, input.reading);
    else
        message = String.Format("Sensor {0} is reading {1:0.0}, threshold is {2:0.0}.", input.readingtype, input.reading, input.threshold);
        
    message = "{\"data\":{\"message\":\""+message+"\"}}";

    return new GcmNotification(message);
}

public class RuleMessage
{
    public string deviceid;
    public string readingtype;
    public object reading;
    public double threshold;
    public DateTime time;
}