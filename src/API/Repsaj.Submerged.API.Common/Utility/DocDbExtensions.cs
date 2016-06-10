using Microsoft.Azure.Documents;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.Utility
{
    public static class DocDbExtensions
    {
        public static JObject ToJObject(this Document doc)
        {
            using (MemoryStream ms = new MemoryStream())
            using (StreamReader sr = new StreamReader(ms))
            {
                doc.SaveTo(ms);
                ms.Position = 0;
                string json = sr.ReadToEnd();
                return JObject.Parse(json);
            }
        }
    }
}
