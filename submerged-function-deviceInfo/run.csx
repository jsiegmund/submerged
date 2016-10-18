#r "System.Configuration"
#r "Newtonsoft.Json"

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Microsoft.Azure.Devices;
//using Microsoft.IdentityModel.Clients.ActiveDirectory;

public static async void Run(string myEventHubMessage, TraceWriter log)
{
    log.Info($"C# Event Hub trigger function processed a message: {myEventHubMessage}");

    string apiClientId = System.Configuration.ConfigurationManager.AppSettings["API_CLIENT_ID"];
    string apiClientSecret = System.Configuration.ConfigurationManager.AppSettings["API_CLIENT_SECRET"];
    string apiClientTokenEndpoint = System.Configuration.ConfigurationManager.AppSettings["API_TOKEN_ENDPOINT"];
    string apiDeviceInfoUrl = System.Configuration.ConfigurationManager.AppSettings["API_DEVICEINFO_URL"];
    string apiUpdateRequestUrl = System.Configuration.ConfigurationManager.AppSettings["API_UPDATEREQUEST_URL"];
    
    log.Info($"Getting authentication token from Azure AD");
    //string authToken = await GetAuthAsync(apiClientTokenEndpoint, apiClientId, apiClientSecret, log);
	string authToken = "none";
    log.Info($"Forwarding incoming device message to API");
    
    dynamic eventData = ((JArray)JsonConvert.DeserializeObject(myEventHubMessage))[0];
    string objectType = eventData.ObjectType ?? eventData.objectType ?? eventData.objecttype ?? "";
    if (objectType == "DeviceInfo")
    {
        log.Info("Sending device info to API back-end");
        await SendDeviceInfoAsync(apiDeviceInfoUrl, myEventHubMessage, authToken, log);
        log.Info($"Device info sent to API back-end");
    }
    else if (objectType == "UpdateRequest")
    {
        log.Info("Sending device update request to API back-end");
        await SendUpdateRequestAsync(apiUpdateRequestUrl, myEventHubMessage, authToken, log);
        log.Info($"Device update request sent to API back-end");
    }
    else if (objectType == "DeviceThrottleRequest")
    {
        await DisableDevice(myEventHubMessage, log);
    }
    else
    {
        log.Info($"Unknown object type {objectType}");
    }
}

public static async Task DisableDevice(string input, TraceWriter log)
{
    log.Info("Received request to throttle device.");

    try {
        dynamic eventData = ((JArray)JsonConvert.DeserializeObject(input))[0];
        string deviceId = eventData.deviceid;

        string iotHubConnectionString = System.Configuration.ConfigurationManager.AppSettings["iotHub.ConnectionString"];
        var registry = RegistryManager.CreateFromConnectionString(iotHubConnectionString);

        log.Info($"Disabling device {deviceId} in IoT hub.");
        var device = await registry.GetDeviceAsync(deviceId);
        device.Status = DeviceStatus.Disabled;
        await registry.UpdateDeviceAsync(device);

        log.Info("Successfully disabled device.");       
    } 
    catch (Exception ex)
    {
        log.Error("Failure trying to disable a device: " + ex.ToString());
    }
}

public static async Task SendUpdateRequestAsync(string endpoint, string input, string authToken, TraceWriter log)
{
    using (var client = new HttpClient())
    {
        dynamic eventData = ((JArray)JsonConvert.DeserializeObject(input))[0];
        string deviceInfoJson = JsonConvert.SerializeObject(eventData);
        
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var content = new StringContent(deviceInfoJson, System.Text.Encoding.UTF8, "application/json");
        //var values = new Dictionary<string, string> {
        //    { "deviceInfo", deviceInfoJson },
        //};
    
        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
    
        //var content = new FormUrlEncodedContent(values);
        log.Info($"Calling submerged API @ {endpoint}: {deviceInfoJson}");
        var response = await client.PostAsync(endpoint, content);
        log.Info($"Received response from API: {response.StatusCode}");
    }
}

public static async Task SendDeviceInfoAsync(string endpoint, string input, string authToken, TraceWriter log)
{
    using (var client = new HttpClient())
    {
        dynamic eventData = ((JArray)JsonConvert.DeserializeObject(input))[0];
        string deviceInfoJson = JsonConvert.SerializeObject(eventData);
        
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var content = new StringContent(deviceInfoJson, System.Text.Encoding.UTF8, "application/json");
        //var values = new Dictionary<string, string> {
        //    { "deviceInfo", deviceInfoJson },
        //};
    
        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
    
        //var content = new FormUrlEncodedContent(values);
        log.Info($"Calling submerged API @ {endpoint}: {deviceInfoJson}");
        var response = await client.PostAsync(endpoint, content);
        log.Info($"Received response from API: {response.StatusCode}");
    }
}

public static async Task<string> GetAuthAsync(string endpoint, string clientId, string clientSecret, TraceWriter log)
{
    try {
        using (var client = new HttpClient())
        {
            var values = new Dictionary<string, string> {
                { "grant_type", "client_credentials" },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "resource", "https://neptune-mobileapi.azurewebsites.net" }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(endpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();
            log.Info($"Received response from AD: {responseString}");
            
            dynamic responseObject = JsonConvert.DeserializeObject(responseString);
            return responseObject.access_token;
        }
    } 
    catch (Exception ex)
    {
        log.Info($"Failure getting authentication token from Azure AD: {ex}");
        throw ex;
    }
}