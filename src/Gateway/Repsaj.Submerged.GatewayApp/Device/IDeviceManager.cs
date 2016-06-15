using Repsaj.Submerged.GatewayApp.Models;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Device
{
    public delegate void UpdateLog(string log);

    public interface IDeviceManager : IDisposable, INotifyPropertyChanged
    {
        event UpdateLog NewLogLine;
        event Action<IEnumerable<Sensor>> SensorDataChanged;
        event Action<IEnumerable<Module>> ModuleDataChanged;

        Task Init();
    }
}
