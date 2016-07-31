using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.API.Tests.Integration
{
    [TestClass]
    public class SubscriptionIntegrationTest
    {
        static SubscriptionIntegrationContext _context;
        static bool cleanupRequired = false;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _context = new SubscriptionIntegrationContext();

            // perform initialization
            _context.Initialize();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            try
            {
                _context.Integration_Subscription_DeleteSubscription().Wait();
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
        public async Task Integration_Subscription()
        {

            // perform the actions we want to test 
            await _context.Integration_Subscription_CreateSubscription();
            cleanupRequired = true;

            await _context.Integration_Subscription_AddDevices();
            await _context.Integration_Subscription_AddModules();
            await _context.Integration_Subscription_AddRelays();
            await _context.IntegrationSubscription_GetRelays();
            await _context.Integration_Subscription_AddSensors();

            await _context.Integration_Subscription_UpdateSubscription();
            await _context.Integration_Subscription_UpdateDevices();
            await _context.Integration_Subscription_UpdateModules();
            await _context.Integration_Subscription_UpdateSensors();
            await _context.Integration_Subscription_UpdateRelays();

            await _context.Integration_Subscription_DeleteRelays();
            await _context.Integration_Subscription_DeleteSensors();
            await _context.Integration_Subscription_DeleteModules();
            await _context.Integration_Subscription_DeleteDevices();

            await _context.Integration_Subscription_DeleteSubscription();
        }
    }
}
