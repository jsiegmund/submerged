using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Models.ConfigurationModels
{
    public class LedenetModuleConfiguration
    {
        public string Device { get; set; }
        public List<LedenetPointInTime> PointsInTime { get; set; }
    }

    public class LedenetPointInTime
    {
        public string Time { get; set; }
        public int FadeIn { get; set; }
        public int Level { get; set; }
        public string Color { get; set; }
        public int White { get; set; }
    }
}
