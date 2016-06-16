using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repsaj.Submerged.Common.Helpers;

namespace Repsaj.Submerged.API.Tests.UnitTests
{
    [TestClass]
    public class HelperTests
    {
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
