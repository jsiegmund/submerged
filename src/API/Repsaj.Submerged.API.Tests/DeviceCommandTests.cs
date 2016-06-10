using Repsaj.Submerged.Common.DeviceSchema;
using Repsaj.Submerged.Common.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;

namespace Repsaj.Submerged.APITests
{
    [TestClass]
    public class DeviceCommandTests : TestBase
    {

        [TestMethod]
        public void CanAddDevice()
        {
            dynamic device;

            device = DeviceSchemaHelper.BuildDeviceStructure(DeviceId,
                false, Guid.NewGuid().ToString());

            DeviceLogic.AddDeviceAsync(device).Wait();
        }

        [TestMethod]
        public void CanSendCommand()
        {
            Dictionary<string, object> commandParams = new Dictionary<string, object>();
            commandParams.Add("RelayNumber", 1);
            commandParams.Add("RelayState", true);

            DeviceLogic.SendCommandAsync(DeviceId, "SwitchRelay", commandParams).Wait();
        }
    }
}
