using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Autofac;
using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Infrastructure.Repository;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using System.Threading.Tasks;
using Repsaj.Submerged.API.Tests.Helpers;
using System.Linq;
using Repsaj.Submerged.Common.Utility;

namespace Repsaj.Submerged.API.Tests.Integration
{
    [TestClass]
    public class DeviceTelemetryLogicTests
    {
        private IContainer _autofacContainer;

        DateTimeOffset _testDateUTC = DateTimeOffset.Parse("2016-08-09T12:14:42.1150000Z");
        int _timeOffsetSeconds = 7200;


        [TestInitialize]
        public void Init()
        {
            var builder = new ContainerBuilder();

            // register configuration provider
            builder.RegisterType<ConfigurationProvider>().As<IConfigurationProvider>();
            builder.RegisterType<DocDbOperations>().As<IDocDbOperations>();

            // Repositories
            builder.RegisterType<SubscriptionRepository>().As<ISubscriptionRepository>();
            builder.RegisterType<DeviceTelemetryRepository>().As<IDeviceTelemetryRepository>();
            builder.RegisterType<DeviceTelemetryLogic>().As<IDeviceTelemetryLogic>();

            // build the container and assign it to our test class for use by the individual test methods
            var container = builder.Build();
            _autofacContainer = container;
        }

        [TestMethod]
        public async Task Integration_Logic_LoadLatestDeviceTelemetry()
        {
            IDeviceTelemetryLogic deviceTelemetryLogic = _autofacContainer.Resolve<IDeviceTelemetryLogic>();
            var result = await deviceTelemetryLogic.LoadLatestDeviceTelemetryAsync(TestConfigHelper.DeviceId);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.DeviceId, TestConfigHelper.DeviceId);
            Assert.AreEqual(result.EventEnqueuedUTCTime, DateTime.Parse("2016-08-09T12:14:42.1150000Z"));

            // TODO: adjust test to new telemetry model
            var temp1 = result.SensorData.SingleOrDefault(s => s.SensorName == "temperature1");
            Assert.IsNotNull(temp1);
            Assert.AreEqual(24.521834035414194, temp1.Value);

            var temp2 = result.SensorData.SingleOrDefault(s => s.SensorName == "temperature2");
            Assert.IsNotNull(temp2);
            Assert.AreEqual(22.224247227294484, temp2.Value);

            var pH = result.SensorData.SingleOrDefault(s => s.SensorName == "pH");
            Assert.IsNotNull(temp2);
            Assert.AreEqual(7.0419647501045679, temp2.Value);
        }

        [TestMethod]
        public async Task Integration_Logic_LoadDeviceTelemetryReportDataLastThreeHours()
        {
            IDeviceTelemetryLogic deviceTelemetryLogic = _autofacContainer.Resolve<IDeviceTelemetryLogic>();
            var result = await deviceTelemetryLogic.LoadDeviceTelemetryReportDataLastThreeHoursAsync(TestConfigHelper.DeviceId, _testDateUTC, _timeOffsetSeconds);

            string[] dataLabels = result.DataLabels.ToArray();
            Assert.AreEqual("11:14", dataLabels[0]);
            Assert.AreEqual("11:44", dataLabels[1]);
            Assert.AreEqual("12:14", dataLabels[2]);
            Assert.AreEqual("12:44", dataLabels[3]);
            Assert.AreEqual("13:14", dataLabels[4]);
            Assert.AreEqual("13:44", dataLabels[5]);

            string[] serieLabels = result.SerieLabels.ToArray();
            Assert.AreEqual("Temperature 1", serieLabels[0]);
            Assert.AreEqual("Temperature 2", serieLabels[1]);
            Assert.AreEqual("pH", serieLabels[2]);

            double?[] expected_avgTemp1 = { 24.072402533255094, 24.007520887150513, 24.021684693182667, 24.162925279619913, 24.025056556231871, 23.91205540060081 };
            double?[] expected_avgTemp2 = { 19.99892472499, 20.017017501724741, 19.910641956877569, 20.633638578374082, 19.981845252530366, 19.927630016088894 };
            double?[] expected_avgpH = { 6.9740753521090726, 6.98278449510354, 6.96690423963913, 7.0616476512242352, 6.9476928292830511, 7.0404633118124469 };

            double?[] temp1Serie = result.DataSeries.First().ToArray();
            Assert.AreEqual(6, temp1Serie.Length);
            CollectionAssert.AreEqual(expected_avgTemp1, temp1Serie);

            double?[] temp2Serie = result.DataSeries.Skip(1).First().ToArray();
            Assert.AreEqual(6, temp2Serie.Length);
            CollectionAssert.AreEqual(expected_avgTemp2, temp2Serie);

            double?[] pHSerie = result.DataSeries.Skip(2).First().ToArray();
            Assert.AreEqual(6, pHSerie.Length);
            CollectionAssert.AreEqual(expected_avgpH, pHSerie);
        }

        [TestMethod]
        public async Task Integration_Logic_LoadDeviceTelemetryReportDataPerDayAsync()
        {
            IDeviceTelemetryLogic deviceTelemetryLogic = _autofacContainer.Resolve<IDeviceTelemetryLogic>();
            var result = await deviceTelemetryLogic.LoadDeviceTelemetryReportDataPerDayAsync(TestConfigHelper.DeviceId, _testDateUTC, _timeOffsetSeconds);

            double?[] expected_avgTemp1 = { 24.055886440470758, 23.733924151351953, 23.926496063247246, 24.098165850705243, 24.283470662093467, 23.850280979112849, 24.018213214205993, 23.950887654535887, 24.039224108660537, 23.907460981704041, 24.029227402889749, 23.858773566755513, 24.083601104212118, 23.965540161379085, 24.027713748949772, 24.067254018108621, 24.07225713889996, 23.921598245896522, 24.116800528850163, 24.121803649641475, 23.8867042419982, 23.973517506371035, null, null };
            double?[] expected_avgTemp2 = { 20.170963196512766, 19.969256838963336, 20.186654996718275, 19.811274168808605, 19.985786735578028, 19.769531744036911, 20.011914479862241, 19.978322345869763, 19.852941517960094, 19.952118846837173, 19.961945330434769, 20.093039279614128, 19.9553176855926, 20.087250403596052, 20.202881188497983, 19.970172740932011, 19.92812524635568, 19.894533112363202, 19.903369304213378, 20.027988476303715, 20.070264606808124, 19.827577480306338, null, null };
            double?[] expected_avgPH = { 7.0109773558149948, 7.03424153689027, 7.0160559819620367, 7.0282916921788336, 6.990831005252355, 7.0035562588942968, 7.02783244949199, 7.0073028113261362, 7.0195385215429313, 6.9954503887125536, 7.0026778682613173, 6.9774600636202191, 6.9877046774643041, 6.9755911796705767, 7.0121102525536489, 7.0042283004821408, 6.9964640106989373, 6.975934372533084, 7.0408177688442244, 7.0330534790610209, 7.0076332725154389, 6.9816905998725884, null, null };

            Assert.AreEqual(24, result.DataLabels.Count());

            int temp1Index = Array.IndexOf(result.SerieLabels.ToArray(), "Temperature 1");
            double?[] values_temp1 = result.DataSeries.Skip(temp1Index).First().ToArray();
            int temp2Index = Array.IndexOf(result.SerieLabels.ToArray(), "Temperature 2");
            double?[] values_temp2 = result.DataSeries.Skip(temp2Index).First().ToArray();
            int pHIndex = Array.IndexOf(result.SerieLabels.ToArray(), "pH");
            double?[] values_pH = result.DataSeries.Skip(pHIndex).First().ToArray();

            // assert the number of values is the same
            Assert.AreEqual(expected_avgTemp1.Count(), values_temp1.Count());
            Assert.AreEqual(expected_avgTemp2.Count(), values_temp2.Count());
            Assert.AreEqual(expected_avgPH.Count(), values_pH.Count());

            CollectionAssert.AreEqual(expected_avgTemp1, values_temp1);
            CollectionAssert.AreEqual(expected_avgTemp2, values_temp2);
            CollectionAssert.AreEqual(expected_avgPH, values_pH);
        }

        [TestMethod]
        public async Task Integration_Logic_LoadDeviceTelemetryReportDataPerHourAsync()
        {
            IDeviceTelemetryLogic deviceTelemetryLogic = _autofacContainer.Resolve<IDeviceTelemetryLogic>();
            var result = await deviceTelemetryLogic.LoadDeviceTelemetryReportDataPerHourAsync(TestConfigHelper.DeviceId, _testDateUTC, _timeOffsetSeconds);

            double?[] values_temp1 = result.DataSeries.First().ToArray();
            double?[] values_temp2 = result.DataSeries.Skip(1).First().ToArray();
            double?[] values_pH = result.DataSeries.Skip(2).First().ToArray();

            Assert.AreEqual(60, result.DataLabels.Count());
            Assert.AreEqual(60, values_temp1.Count());
            Assert.AreEqual(60, values_temp2.Count());
            Assert.AreEqual(60, values_pH.Count());
        }
    }
}