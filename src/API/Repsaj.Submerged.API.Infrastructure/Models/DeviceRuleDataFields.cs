using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Models
{
    public static class DeviceRuleDataFields
    {
        public static string Temperature1
        {
            get
            {
                return "temperature1";
            }
        }

        public static string Temperature2
        {
            get
            {
                return "temperature2";
            }
        }

        public static string pH
        {
            get
            {
                return "pH";
            }
        }

        private static List<string> _availableDataFields = new List<string>
        {
            Temperature1, Temperature2, pH
        };

        public static List<string> GetListOfAvailableDataFields()
        {
            return _availableDataFields;
        }
    }
}
