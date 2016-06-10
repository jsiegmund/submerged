using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Models
{
    /// <summary>
    /// The supported types that <see cref="string" /> proprty values 
    /// represent.
    /// </summary>
    public enum PropertyType
    {
        /// <summary>
        /// The <see cref="string" /> property represents a 
        /// <see cref="string" />.
        /// </summary>
        String = 0,

        /// <summary>
        /// The <see cref="string" /> property represents a 
        /// <see cref="DateTime" />.
        /// </summary>
        DateTime,

        /// <summary>
        /// The <see cref="string" /> property represents a
        /// <see cref="short" />, <see cref="int" />, or <see cref="long" />.
        /// </summary>
        Integer,

        /// <summary>
        /// The <see cref="string" /> property represents a 
        /// <see cref="single" /> or <see cref="double" />.
        /// </summary>
        Real,

        /// <summary>
        /// The <see cref="string" /> property represents a Status value.
        /// </summary>
        Status
    }
}
