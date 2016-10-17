using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules
{
    public interface ISensorModule : IModuleConnection
    {
        Task<IEnumerable<SensorTelemetryModel>> RequestSensorData();
        IEnumerable<Sensor> Sensors { get; }
    }
}
