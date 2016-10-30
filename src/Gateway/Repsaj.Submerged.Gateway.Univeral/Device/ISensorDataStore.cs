using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repsaj.Submerged.GatewayApp.Universal.Models;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules
{
    public interface ISensorDataStore
    {
        void ProcessData(ISensorModule moduleName, IEnumerable<SensorTelemetryModel> moduleData);
        IEnumerable<SensorTelemetryModel> GetData();
    }
}
