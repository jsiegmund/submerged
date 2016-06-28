using System;

public async static void Run(TimerInfo myTimer, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
    
    using (var client = new HttpClient())
    {
        var endpoint = "http://repsaj-domain-blog.azurewebsites.net";
        var response = await client.GetAsync(endpoint);
        log.Info($"Received response: {response.StatusCode}");
    }
    
    using (var client = new HttpClient())
    {
        var endpoint = "http://repsaj-domain-orchard.azurewebsites.net";
        var response = await client.GetAsync(endpoint);
        log.Info($"Received response: {response.StatusCode}");
    }

}