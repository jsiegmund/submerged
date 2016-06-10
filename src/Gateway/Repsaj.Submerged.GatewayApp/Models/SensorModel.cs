using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Models
{
    public class SensorModel
    {
        public string Name { get; set; }
        public object Reading { get; set; }

        public string ReadingAsText
        {
            get
            {
                string formattedText;

                if (Reading is double || Reading is float)
                    formattedText = String.Format("{0:0.00}", Reading);
                else
                    formattedText = Reading.ToString();

                return formattedText;
            }
        }
    }
}
