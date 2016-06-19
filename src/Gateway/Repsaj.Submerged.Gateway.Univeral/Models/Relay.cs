using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Models
{
    public class Relay
    {
        public int RelayNumber { get; set; }
        public string Name { get; set; }
        public Boolean State { get; set; }
        public int? OrderNumber { get; set; }
        public string Module { get; set; }
    }
}
