using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using Repsaj.Submerged.Common.DeviceSchema;
using Repsaj.Submerged.API.Tests.Helpers;

namespace Repsaj.Submerged.API.Tests.Integration
{
    [TestClass]
    public class DocDbTests
    {
        //[TestMethod]
        //public async Task CanSaveNewDocument()
        //{
        //    dynamic jsonObject = new JObject();
        //    jsonObject.Type = "Test";

        //    await DocDbOperations.SaveNewDocumentAsync(jsonObject);
        //}

        //[TestMethod]
        //public async Task CanUpdateDocument()
        //{
        //    dynamic device = await DeviceLogic.GetDeviceAsync(TestConfigHelper.DeviceId);
        //    var result = await DocDbOperations.UpdateDocumentAsync(device, device.Id);
        //    Assert.IsNotNull(result);
        //}
    }
}