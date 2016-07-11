using Repsaj.Submerged.Common.SubscriptionSchema;
using Repsaj.Submerged.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.API.Tests.Helpers
{
    public static class TestClasses
    {
        public static TankLog GetTankLog()
        {
            TankLog log = new TankLog(TestStatics.tankLog_tankId)
            {
                Description = TestStatics.tankLog_description,
                LogType = TestStatics.tankLog_logType,
                Title = TestStatics.tankLog_title
            };

            return log;
        }
    }
}
