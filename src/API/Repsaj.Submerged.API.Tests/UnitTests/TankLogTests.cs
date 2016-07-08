using Autofac.Extras.Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repsaj.Submerged.API.Tests.Helpers;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using Repsaj.Submerged.Infrastructure.Exceptions;
using Repsaj.Submerged.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.API.Tests.UnitTests
{
    [TestClass]
    public class TankLogTests
    {
        [TestMethod]
        public async Task Save_TankLog_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var tankLogLogic = autoMock.Create<TankLogLogic>();
                var logLine = TestClasses.GetTankLog();
                await tankLogLogic.SaveTankLogAsync(logLine);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SubscriptionValidationException))]
        public async Task Save_TankLog_Failure_NoTitle()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var tankLogLogic = autoMock.Create<TankLogLogic>();
                var logLine = TestClasses.GetTankLog();
                logLine.Title = "";
                await tankLogLogic.SaveTankLogAsync(logLine);
            }
        }


        [TestMethod]
        [ExpectedException(typeof(SubscriptionValidationException))]
        public async Task Save_TankLog_Failure_NoDescription()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var tankLogLogic = autoMock.Create<TankLogLogic>();
                var logLine = TestClasses.GetTankLog();
                logLine.Description = "";
                await tankLogLogic.SaveTankLogAsync(logLine);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SubscriptionValidationException))]
        public async Task Save_TankLog_Failure_NoType()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var tankLogLogic = autoMock.Create<TankLogLogic>();
                var logLine = TestClasses.GetTankLog();
                logLine.LogType = "";
                await tankLogLogic.SaveTankLogAsync(logLine);
            }
        }


        [TestMethod]
        public async Task Get_TankLog_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedTankLog(autoMock);

                var tankLogLogic = autoMock.Create<TankLogLogic>();

                var result = await tankLogLogic.GetTankLogAsync(TestStatics.tankLog_tankId, TestStatics.tankLog_id);

                Assert.IsNotNull(result);
                Assert.AreEqual(TestStatics.tankLog_title, result.Title);
                Assert.AreEqual(TestStatics.tankLog_description, result.Description);
                Assert.AreEqual(TestStatics.tankLog_id, result.LogId);
                Assert.AreEqual(TestStatics.tankLog_logType, result.LogType);
                Assert.AreEqual(TestStatics.tankLog_tankId, result.TankId);
            }
        }

        [TestMethod]
        public async Task Get_TankLogs_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedTankLog(autoMock);

                var tankLogLogic = autoMock.Create<TankLogLogic>();

                var result = await tankLogLogic.GetTankLogAsync(TestStatics.tankLog_tankId);
                Assert.AreEqual(1, result.Count());
            }
        }

        [TestMethod]
        public async Task Delete_TankLog_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedTankLog(autoMock);

                var tankLogLogic = autoMock.Create<TankLogLogic>();

                await tankLogLogic.DeleteTankLogAsync(TestStatics.tankLog_tankId, TestStatics.tankLog_id);
            }
        }
    }
}
