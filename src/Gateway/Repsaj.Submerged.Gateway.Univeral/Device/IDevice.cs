using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Device
{
    public interface IDevice
    {
        string DeviceID { get; set; }

        string HostName { get; set; }

        string PrimaryAuthKey { get; set; }

        dynamic DeviceProperties { get; set; }

        dynamic Commands { get; set; }

        List<ITelemetry> TelemetryEvents { get; }

        bool RepeatEventListForever { get; set; }

        void Init(InitialDeviceConfig config);


        Task SendDeviceInfo();

        dynamic GetDeviceInfo();

        Task StartAsync(CancellationToken token);
    }
}
