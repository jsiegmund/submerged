using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Device
{
    public delegate void UpdateLog(string log);

    public interface IDeviceManager : IDisposable
    {
        event UpdateLog NewLogLine;

        event Action<IEnumerable<Module>> ModulesUpdated;
        event Action<IEnumerable<Sensor>> SensorsUpdated;
        event Action<IEnumerable<Relay>> RelaysUpdated;

        event Action AzureConnected;
        event Action AzureDisconnected;

        Task Init();
        Task RequestDeviceUpdate();
    }
}
