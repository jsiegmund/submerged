using Repsaj.Submerged.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.BusinessLogic
{
    public interface ISecurityKeyGenerator
    {
        /// <summary>
        /// Creates a random security key pair
        /// </summary>
        /// <returns>Populated SecurityKeys object</returns>
        SecurityKeys CreateRandomKeys();
    }
}
