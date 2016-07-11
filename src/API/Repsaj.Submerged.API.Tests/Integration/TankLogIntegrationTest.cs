using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.API.Tests.Integration
{
    [TestClass]
    public class TankLogIntegrationTest
    {
        static TankLogIntegrationContext _context;
        static bool cleanupRequired = false;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _context = new TankLogIntegrationContext();

            // perform initialization
            _context.Initialize();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            try
            {
                if (cleanupRequired)
                    _context.Integration_TankLog_DeleteLog().Wait();
            }
            catch (Exception)
            {
                // ignore any error during cleaning up
            }
            finally
            {
                _context.Dispose();
                _context = null;
            }
        }

        [TestMethod]
        public async Task Integration_TankLog()
        {
            // perform the actions we want to test 
            var success = await _context.Integration_TankLog_CreateLog();
            Assert.IsTrue(success);
            cleanupRequired = true;

            // try to fetch the log
            var log = await _context.Integration_TankLog_GetLog();
            Assert.IsNotNull(log);

            // fetch all of the logs for this tank, make sure it reads 1
            var logs = await _context.Integration_TankLog_GetTankLog();
            Assert.AreEqual(1, logs.Count());

            await _context.Integration_TankLog_DeleteLog();
            cleanupRequired = false;

        }
    }
}
