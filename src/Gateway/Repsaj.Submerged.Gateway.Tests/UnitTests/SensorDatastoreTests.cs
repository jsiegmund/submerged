using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Repsaj.Submerged.GatewayApp.Universal.Modules;
using Repsaj.Submerged.GatewayApp.Universal.Modules.Simulated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Gateway.Tests.UnitTests
{
    [TestClass]
    public class SensorDatastoreTests
    {
        private static string _moduleName1 = "Test Module 1";
        private static string _moduleName2 = "Test Module 2";
        private static string _sensorName1 = "Test Sensor 1";
        private static string _sensorName2 = "Test Sensor 2";
        private static ISensorModule _sensorModule;
        private static Sensor[] _sensors;

        [ClassInitialize]
        public static void Intialize(TestContext context)
        {
            _sensors = new Sensor[]
            {
                new Sensor()
                {
                     Name = _sensorName1,
                     SensorType = SensorTypes.PH
                },
                new Sensor()
                {
                    Name = _sensorName2,
                    SensorType = SensorTypes.TEMPERATURE
                }
            };

            _sensorModule = new SimulatedSensorModuleConnection(_moduleName1, _sensors);
        }

        private SensorTelemetryModel[] FakeTelemetry()
        {
            return new SensorTelemetryModel[]
            {
                            new SensorTelemetryModel(_sensorName1, 6.5),
                            new SensorTelemetryModel(_sensorName2, 23)
            };
        }

        [TestMethod]
        public void Can_ProcessSensorTelemetryNull_Success()
        {
            ISensorDataStore datastore = new SensorDatastore();
            datastore.ProcessData(_sensorModule, null);
        }

        [TestMethod]
        public void Can_ProcessSensorTelemetrySecondNull_Success()
        {
            var telemetry = FakeTelemetry();
            ISensorDataStore datastore = new SensorDatastore();
            datastore.ProcessData(_sensorModule, telemetry);
            datastore.ProcessData(_sensorModule, null);

            var result = datastore.GetData();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            
            // since the second value was null, the result should be the same value as the first record
            Assert.AreEqual(telemetry[0].Value, result.Single(r => r.SensorName == telemetry[0].SensorName).Value);
            Assert.AreEqual(telemetry[1].Value, result.Single(r => r.SensorName == telemetry[1].SensorName).Value);
        }

        [TestMethod]
        public void Can_ProcessSensorTelemetry_Success()
        {
            ISensorDataStore datastore = new SensorDatastore();
            datastore.ProcessData(_sensorModule, FakeTelemetry());
        }

        [TestMethod]
        public void Can_GetData_Success()
        {
            ISensorDataStore datastore = new SensorDatastore();
            datastore.ProcessData(_sensorModule, FakeTelemetry());
            var result = datastore.GetData();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }
    }
}
