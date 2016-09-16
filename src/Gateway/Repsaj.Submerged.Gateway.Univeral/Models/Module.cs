using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Models
{
    public class Module
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string ConnectionString { get; set; }
        public string ModuleType { get; set; }
        public string Status { get; set; }
        public int? DisplayOrder { get; set; }
        public JObject Configuration { get; set; }

        public static Module BuildModule(string name, string connectionstring, string moduleType)
        {
            Module model = new Module()
            {
                Name = name,
                ConnectionString = connectionstring,
                ModuleType = moduleType
            };

            return model;
        }
    }
}
