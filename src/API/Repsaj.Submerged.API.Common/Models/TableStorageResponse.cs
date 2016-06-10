using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.Models
{
    public class TableStorageResponse<T>
    {
        public T Entity { get; set; }
        public TableStorageResponseStatus Status { get; set; }
    }

    public enum TableStorageResponseStatus
    {
        Successful, ConflictError, UnknownError, DuplicateInsert, NotFound
    }
}
