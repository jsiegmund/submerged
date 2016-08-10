using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Models
{
    public class DeviceInstallation
    {
        public string InstallationId { get; set; }
        public string Handle { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public string Platform { get; set; }
    }
}
