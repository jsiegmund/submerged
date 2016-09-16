using Repsaj.Submerged.GatewayApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Modules
{
    public interface ISensorModule
    {
        IEnumerable<SensorTelemetryModel> RequestSensorData();
    }
}
