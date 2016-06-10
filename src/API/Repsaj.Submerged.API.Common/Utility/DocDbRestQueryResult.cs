using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.Utility
{
    public class DocDbRestQueryResult
    {
        public JArray ResultSet { get; set; }
        public int TotalResults { get; set; }
        public string ContinuationToken { get; set; }
    }
}
