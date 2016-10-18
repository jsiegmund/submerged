using System;
using Microsoft.Azure.Devices;

public static async Task Run(TimerInfo myTimer, TraceWriter log)
{
    try {
        string iotHubConnectionString = System.Configuration.ConfigurationManager.AppSettings["iotHub.ConnectionString"];
        var registry = RegistryManager.CreateFromConnectionString(iotHubConnectionString);

        log.Info($"Getting the device registry list from IoT hub.");
        var devices = await registry.GetDevicesAsync(1000);

        foreach (var device in devices)
        {
            if (device.Status == DeviceStatus.Disabled)
            {
                device.Status = DeviceStatus.Enabled;
                await registry.UpdateDeviceAsync(device);
                log.Info($"Re-enabled device {device.Id}.");
            }
        }

        log.Info("Successfully re-enabled all devices.");       
    } 
    catch (Exception ex)
    {
        log.Error("Failure trying to disable a device: " + ex.ToString());
    }    
}