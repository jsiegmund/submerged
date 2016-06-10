﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.SubscriptionSchema
{
    public class ModuleModel
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string ModuleType { get; set; }
        public string Status { get; set; }
        public int? DisplayOrder { get; set; }

        public static ModuleModel BuildModule(string name, string connectionstring, string moduleType)
        {
            ModuleModel model = new ModuleModel()
            {
                Name = name,
                ConnectionString = connectionstring,
                ModuleType = moduleType
            };

            return model;
        }
    }
}
