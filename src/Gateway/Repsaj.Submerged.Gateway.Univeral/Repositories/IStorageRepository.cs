using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Repositories
{
    public interface IStorageRepository
    {
        Task<dynamic> GetStoredObject(string filename);
        Task SaveObjectToStorage(dynamic obj, string filename);
    }
}
