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
        private static string _sensorName3 = "Test Sensor 3";
        private static ISensorModule _sensorModule;
        private static Sensor[] _sensors;

        private static double _baseTemp1 = 24d;
        private static double _basePH = 7.0d;

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
                },
                new Sensor()
                {
                    Name = _sensorName3,
                    SensorType = SensorTypes.MOISTURE
                }
            };

            _sensorModule = new SimulatedSensorModuleConnection(_moduleName1, _sensors);
        }

        private SensorTelemetryModel[][] GenerateFakeTelemetry(int readingCount)
        {
            List<SensorTelemetryModel[]> result = new List<SensorTelemetryModel[]>();

            Random rand = new Random();

            for (int i = 0; i < readingCount; i++)
            {
                double currentTemp = _baseTemp1 + rand.NextDouble() * 5 - 2.5;
                double currentPH = _basePH + rand.NextDouble() * 1.2 - 0.6;

                SensorTelemetryModel[] telemetry = new SensorTelemetryModel[]
                {
                    new SensorTelemetryModel(_sensorName1, currentPH),
                    new SensorTelemetryModel(_sensorName2, currentTemp),
                    new SensorTelemetryModel(_sensorName3, true)
                };

                result.Add(telemetry);
            }

            return result.ToArray();
        }

        private SensorTelemetryModel[][] GenerateFakeNullTelemetry(int readingCount)
        {
            List<SensorTelemetryModel[]> result = new List<SensorTelemetryModel[]>();

            for (int i = 0; i < readingCount; i++)
            {
                SensorTelemetryModel[] telemetry = new SensorTelemetryModel[]
                {
                    new SensorTelemetryModel(_sensorName1, null),
                    new SensorTelemetryModel(_sensorName2, null),
                    new SensorTelemetryModel(_sensorName3, null)
                };

                result.Add(telemetry);
            }

            return result.ToArray();
        }

        [TestMethod]
        public void SensorDatastore_CanProcessSensorTelemetryNull_Success()
        {
            ISensorDataStore datastore = new SensorDatastore();
            datastore.ProcessData(_sensorModule, null);
        }

        [TestMethod]
        public void SensorDatastore_CanProcessSensorTelemetrySecondNull_Success()
        {
            var telemetry = GenerateFakeTelemetry(1)[0];

            ISensorDataStore datastore = new SensorDatastore();
            datastore.ProcessData(_sensorModule, telemetry);
            datastore.ProcessData(_sensorModule, null);

            var result = datastore.GetData();
            Assert.IsNotNull(result);
            Assert.AreEqual(telemetry.Length, result.Count());
            
            // since the second value was null, the result should be the same value as the first record
            Assert.AreEqual(telemetry[0].Value, result.Single(r => r.SensorName == telemetry[0].SensorName).Value);
            Assert.AreEqual(telemetry[1].Value, result.Single(r => r.SensorName == telemetry[1].SensorName).Value);
        }

        [TestMethod]
        public void SensorDatastore_CanProcessSensorTelemetryMultiple_Success()
        {
            var telemetry = GenerateFakeTelemetry(10);
            ISensorDataStore datastore = new SensorDatastore();

            foreach (var item in telemetry)
                datastore.ProcessData(_sensorModule, item);

            var result = datastore.GetData();
            Assert.IsNotNull(result);
            Assert.AreEqual(telemetry[0].Length, result.Count());
        }


        [TestMethod]
        public void SensorDatastore_CanProcessSensorTelemetry_Success()
        {
            ISensorDataStore datastore = new SensorDatastore();
            datastore.ProcessData(_sensorModule, GenerateFakeTelemetry(1)[0]);
        }

        [TestMethod]
        public void SensorDatastore_CanGetData_Success()
        {
            var telemetry = GenerateFakeTelemetry(1)[0];

            ISensorDataStore datastore = new SensorDatastore();
            datastore.ProcessData(_sensorModule, telemetry);
            var result = datastore.GetData();

            Assert.IsNotNull(result);
            Assert.AreEqual(telemetry.Length, result.Count());
        }

        [TestMethod]
        public void SensorDatastore_CanGetData_WithOverflow_Success()
        {
            var telemetry = GenerateFakeTelemetry(10);
       
            ISensorDataStore datastore = new SensorDatastore();
            foreach (var item in telemetry)
                datastore.ProcessData(_sensorModule, item);

            var result = datastore.GetData();

            Assert.IsNotNull(result);

            // check that the value of a sensor equals the average of the last 6 readings
            var expected = telemetry.Skip(4).Take(6).SelectMany(t => t.Where(x => x.SensorName == _sensorName1)).Average(r => Convert.ToDouble(r.Value));
            var actual = result.Single(t => t.SensorName == _sensorName1).Value;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SensorDatastore_CanGetData_ReadingsDrop_Success()
        {
            IEnumerable<SensorTelemetryModel[]> telemetry = GenerateFakeTelemetry(10);
            var nulltelemetry = GenerateFakeNullTelemetry(10);

            // append the null readings which should cancel out the correct readings
            telemetry = telemetry.Union(nulltelemetry);

            ISensorDataStore datastore = new SensorDatastore();
            foreach (var item in telemetry)
                datastore.ProcessData(_sensorModule, item);

            var result = datastore.GetData();

            Assert.IsNotNull(result);
            Assert.AreEqual(telemetry.First().Length, result.Count());

            // check that the value of a sensor equals the average of the last 6 readings
            var allNull = result.All(r => r.Value == null);
            Assert.IsTrue(allNull);
        }

        [TestMethod]
        public void SensorDatastore_CanGetData_OnlyTwoReadings_Success()
        {
            IEnumerable<SensorTelemetryModel[]> telemetry;

            telemetry = GenerateFakeTelemetry(3);

            ISensorDataStore datastore = new SensorDatastore();
            foreach (var item in telemetry)
                datastore.ProcessData(_sensorModule, item);

            var result = datastore.GetData();

            Assert.IsNotNull(result);
            Assert.AreEqual(telemetry.First().Length, result.Count());

            // check that the value of a sensor equals the average of the last 6 readings
            var expected = telemetry.SelectMany(x => x.Where(y => y.SensorName == _sensorName1 && y.Value != null)).Average(r => Convert.ToDouble(r.Value));
            var actual = result.Single(t => t.SensorName == _sensorName1).Value;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SensorDatastore_CanGetData_NullAndReadings_Success()
        {
            IEnumerable<SensorTelemetryModel[]> telemetry;

            telemetry = GenerateFakeNullTelemetry(4);
            telemetry = telemetry.Union(GenerateFakeTelemetry(2));

            ISensorDataStore datastore = new SensorDatastore();
            foreach (var item in telemetry)
                datastore.ProcessData(_sensorModule, item);

            var result = datastore.GetData();

            Assert.IsNotNull(result);
            Assert.AreEqual(telemetry.First().Length, result.Count());

            // check that the value of a sensor equals the average of the last 6 readings
            var expected = telemetry.SelectMany(x => x.Where(y => y.SensorName == _sensorName1 && y.Value != null)).Average(r => Convert.ToDouble(r.Value));
            var actual = result.Single(t => t.SensorName == _sensorName1).Value;

            Assert.AreEqual(expected, actual);
        }
    }
}
