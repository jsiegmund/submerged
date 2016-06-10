using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.Models
{
    public class SecurityKeys
    {
        public SecurityKeys(string primaryKey, string secondaryKey)
        {
            PrimaryKey = primaryKey;
            SecondaryKey = secondaryKey;
        }

        public string PrimaryKey { get; set; }

        public string SecondaryKey { get; set; }
    }

    public enum SecurityKey
    {
        None = 0,
        [Display(Name = "primary")]
        Primary,
        [Display(Name = "secondary")]
        Secondary
    }
}
