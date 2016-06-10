using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Models
{
    /// <summary>
    /// Model object that extends ActionMapping with additional data
    /// </summary>
    public class ActionMappingExtended : ActionMapping
    {
        public int NumberOfDevices { get; set; }
    }
}
