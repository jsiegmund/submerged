using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repsaj.Submerged.Common.Helpers;
using Repsaj.Submerged.Common.Models;
using System.Collections.Generic;
using System.Linq;

namespace Repsaj.Submerged.API.Tests.UnitTests
{
    [TestClass]
    public class HelperTests
    {
        private GroupedTelemetryModel[] MockGroupedTelemetry(int count, int offset = 0)
        {
            List<GroupedTelemetryModel> result = new List<GroupedTelemetryModel>();

            Random random = new Random();

            for (int i = offset; i < count + offset; i++)
            {
                var sensor1 = new SensorTelemetryModel(TestStatics.sensor_name, random.NextDouble());
                var sensor2 = new SensorTelemetryModel(TestStatics.sensor2_name, random.NextDouble());

                var item = new GroupedTelemetryModel()
                {
                    Key = i,
                    SensorData = new SensorTelemetryModel[] { sensor1, sensor2 }
                };

                result.Add(item);
            }

            return result.ToArray();
        }

        private SensorTelemetryModel[] MockSensorTelemetry(int count, string sensorName)
        {
            List<SensorTelemetryModel> result = new List<SensorTelemetryModel>();

            Random random = new Random();
                
            for (int i = 0; i < count; i++)
            {
                result.Add(new SensorTelemetryModel(sensorName, random.NextDouble()));
            }

            return result.ToArray();
        }

        private void AssertAllItemsAreNull(double? expectedValue, IEnumerable<double?> array)
        {
            foreach (object item in array)
                Assert.AreEqual(expectedValue, item);
        }

        [TestMethod]
        public void Test_Helper_PadTelemetryData_Begin()
        {
            int data_count = 30;
            int data_offset = 0;
            int padded_count = 60;

            // provides data which begins at '2' and stops at '55'
            var testData = MockGroupedTelemetry(data_count, data_offset); 

            IEnumerable<IEnumerable<Double?>> padded = MiscHelper.PadTelemetryData(testData, 0, padded_count);

            // the resulting set should have 
            Assert.AreEqual(2, padded.Count());

            foreach (var serie in padded)
            {
                Assert.AreEqual(padded_count, serie.Count());

                // first [data_offset] items should be null
                AssertAllItemsAreNull(null, serie.Take(data_offset));
                // next [data_count] items should not be null
                CollectionAssert.AllItemsAreNotNull(serie.Skip(data_offset).Take(data_count).ToArray());
                // all subsequent items should be null
                AssertAllItemsAreNull(null, serie.Skip(data_offset + data_count));
            }
        }

        [TestMethod]
        public void Test_Helper_PadTelemetryData_Middle()
        {
            int data_count = 5;
            int data_offset = 5;
            int padded_count = 60;

            // provides data which begins at '2' and stops at '55'
            var testData = MockGroupedTelemetry(data_count, data_offset);

            IEnumerable<IEnumerable<Double?>> padded = MiscHelper.PadTelemetryData(testData, 0, padded_count);

            // the resulting set should have 
            Assert.AreEqual(2, padded.Count());

            foreach (var serie in padded)
            {
                Assert.AreEqual(padded_count, serie.Count());

                // first [data_offset] items should be null
                AssertAllItemsAreNull(null, serie.Take(data_offset));
                // next [data_count] items should not be null
                CollectionAssert.AllItemsAreNotNull(serie.Skip(data_offset).Take(data_count).ToArray());
                // all subsequent items should be null
                AssertAllItemsAreNull(null, serie.Skip(data_offset + data_count));
            }
        }

        [TestMethod]
        public void Test_Helper_PadTelemetryData_End()
        {
            int data_count = 27;
            int data_offset = 33;
            int padded_count = 60;

            // provides data which begins at '2' and stops at '55'
            var testData = MockGroupedTelemetry(data_count, data_offset);

            IEnumerable<IEnumerable<Double?>> padded = MiscHelper.PadTelemetryData(testData, 0, padded_count);

            // the resulting set should have 
            Assert.AreEqual(2, padded.Count());

            foreach (var serie in padded)
            {
                Assert.AreEqual(padded_count, serie.Count());

                // first [data_offset] items should be null
                AssertAllItemsAreNull(null, serie.Take(data_offset));
                // next [data_count] items should not be null
                CollectionAssert.AllItemsAreNotNull(serie.Skip(data_offset).Take(data_count).ToArray());
                // all subsequent items should be null
                AssertAllItemsAreNull(null, serie.Skip(data_offset + data_count));
            }
        }


        [TestMethod]
        public void Test_Helper_PadTelemetryData_Week()
        {
            int data_count = 3;
            int data_offset = 2;
            int padded_count = 7;
            int padded_modulo = 7;

            // provides data which begins at '2' and stops at '55'
            var testData = MockGroupedTelemetry(data_count, data_offset);

            // 1 for 'DayOfWeek' represents Monday
            IEnumerable<IEnumerable<Double?>> padded = MiscHelper.PadTelemetryData(testData, 2, padded_count, padded_modulo);

            // the resulting set should have 
            Assert.AreEqual(2, padded.Count());

            foreach (var serie in padded)
            {
                Assert.AreEqual(padded_count, serie.Count());

                // next [data_count] items should not be null
                CollectionAssert.AllItemsAreNotNull(serie.Take(data_count).ToArray());
                // all subsequent items should be null
                AssertAllItemsAreNull(null, serie.Skip(data_count));
            }
        }

        [TestMethod]
        public void Test_Helper_ProjectHalfHourSegments()
        {
            DateTime start = DateTime.Now;
            int result;

            result = MiscHelper.ProjectHalfHourSegments(start, start.AddMinutes(0));
            Assert.AreEqual(result, 0);

            result = MiscHelper.ProjectHalfHourSegments(start, start.AddMinutes(1));
            Assert.AreEqual(result, 0);

            result = MiscHelper.ProjectHalfHourSegments(start, start.AddMinutes(29));
            Assert.AreEqual(result, 0);

            result = MiscHelper.ProjectHalfHourSegments(start, start.AddMinutes(30));
            Assert.AreEqual(result, 1);

            result = MiscHelper.ProjectHalfHourSegments(start, start.AddMinutes(59));
            Assert.AreEqual(result, 1);

            result = MiscHelper.ProjectHalfHourSegments(start, start.AddMinutes(60));
            Assert.AreEqual(result, 2);

            result = MiscHelper.ProjectHalfHourSegments(start, start.AddMinutes(61));
            Assert.AreEqual(result, 2);

            result = MiscHelper.ProjectHalfHourSegments(start, start.AddMinutes(90));
            Assert.AreEqual(result, 3);

            result = MiscHelper.ProjectHalfHourSegments(start, start.AddHours(3));
            Assert.AreEqual(result, 6);
        }
    }
}
