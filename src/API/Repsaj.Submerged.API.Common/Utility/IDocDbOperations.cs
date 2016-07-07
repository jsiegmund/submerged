using Microsoft.Azure.Documents;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.Utility
{
    public interface IDocDbOperations
    {
        Task<JObject> SaveNewDocumentAsync(dynamic document);
        Task<dynamic> SaveNewDocumentAsync<T>(T document) where T : class;
        Task<JObject> UpdateDocumentAsync(dynamic updatedDocument, string id);
        Task<T> UpdateDocumentAsync<T>(T updatedDocument, string id) where T : class;
        Task DeleteDocumentAsync(string documentId);
        Task<JArray> QueryDocuments(string query, Dictionary<string, Object> queryParams);
        Task<IEnumerable<T>> Query<T>(string query, Dictionary<string, Object> queryParams);
    }
}
