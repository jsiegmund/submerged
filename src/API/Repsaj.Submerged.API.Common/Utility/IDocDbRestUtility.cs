using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.Utility
{
    public interface IDocDbRestUtility
    {
        Task InitializeDatabase();
        Task InitializeCollection();
        //Task<DocDbRestQueryResult> QueryCollectionAsync(
            //string queryString, Dictionary<string, Object> queryParams, int pageSize = -1, string continuationToken = null);
        //Task<JObject> SaveNewDocumentAsync(dynamic document);
        //Task<JObject> UpdateDocumentAsync(dynamic updatedDocument);
        //Task DeleteDocumentAsync(dynamic document);
    }
}
