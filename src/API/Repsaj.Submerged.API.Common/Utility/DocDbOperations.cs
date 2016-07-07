using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json.Linq;
using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Common.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.Utility
{
    public class DocDbOperations : IDocDbOperations
    {
        Microsoft.Azure.Documents.Client.DocumentClient _client;

        private readonly string _docDbEndpoint;
        private readonly string _docDbKey;
        private readonly string _dbName;
        private readonly string _collectionName;
        private Uri _dbUri;

        public DocDbOperations(IConfigurationProvider configProvider)
        {
            _docDbEndpoint = configProvider.GetConfigurationSettingValue("docdb.EndpointUrl");
            _docDbKey = configProvider.GetConfigurationSettingValue("docdb.PrimaryAuthorizationKey");
            _dbName = configProvider.GetConfigurationSettingValue("docdb.DatabaseId");
            _collectionName = configProvider.GetConfigurationSettingValue("docdb.DocumentCollectionId");

            _client = new Microsoft.Azure.Documents.Client.DocumentClient(new Uri(_docDbEndpoint), _docDbKey);
            _dbUri = UriFactory.CreateDocumentCollectionUri(_dbName, _collectionName);
        }

        public Task<JArray> QueryDocuments(string query, Dictionary<string, Object> queryParams)
        {
            SqlParameter[] parameterArray = queryParams.Select(kv => new SqlParameter(kv.Key, kv.Value)).ToArray();
            SqlParameterCollection parameters = new SqlParameterCollection(parameterArray);
            SqlQuerySpec querySpec = new SqlQuerySpec(query, parameters);

            return Task.Run<JArray>(() =>
            {
                IQueryable<Document> documents = _client.CreateDocumentQuery<Document>(_dbUri, querySpec);

                JArray result = new JArray();
                foreach (Document doc in documents)
                {
                    result.Add(doc.ToJObject());
                }

                return result;
            });
        }

        public Task<IEnumerable<T>> Query<T>(string query, Dictionary<string, Object> queryParams)
        {
            SqlParameter[] parameterArray = queryParams.Select(kv => new SqlParameter(kv.Key, kv.Value)).ToArray();
            SqlParameterCollection parameters = new SqlParameterCollection(parameterArray);
            SqlQuerySpec querySpec = new SqlQuerySpec(query, parameters);

            return Task.Run<IEnumerable<T>>(() =>
            {
                List<T> result = _client.CreateDocumentQuery<T>(_dbUri, querySpec).ToList();
                return result;
            });
        }

        public async Task<JObject> SaveNewDocumentAsync(dynamic document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            Document doc = await _client.CreateDocumentAsync(_dbUri, document);
            return doc.ToJObject();
        }


        public async Task<dynamic> SaveNewDocumentAsync<T>(T document) where T : class
        {
            Document doc = await _client.CreateDocumentAsync(_dbUri, document);
            return doc;
        }

        /// <summary>
        /// Update the record for an existing document.
        /// </summary>
        /// <param name="updatedDocument"></param>
        /// <returns></returns>
        public async Task<JObject> UpdateDocumentAsync(dynamic updatedDocument, string id)
        {
            if (updatedDocument == null)
            {
                throw new ArgumentNullException(nameof(updatedDocument));
            }

            string docId = SchemaHelper.GetDocDbId(updatedDocument);
            Document doc = await _client.ReplaceDocumentAsync(docId, updatedDocument);
            return doc.ToJObject();
        }

        public async Task<T> UpdateDocumentAsync<T>(T updatedDocument, string id) where T : class
        {
            if (updatedDocument == null)
            {
                throw new ArgumentNullException(nameof(updatedDocument));
            }

            Uri docUri = UriFactory.CreateDocumentUri(_dbName, _collectionName, id);
            await _client.ReplaceDocumentAsync(docUri, updatedDocument);
            return updatedDocument;
        }

        /// <summary>
        /// Remove a document from the DocumentDB. If it succeeds the method will return asynchronously.
        /// If it fails for any reason it will let any exception thrown bubble up.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public async Task DeleteDocumentAsync(string documentId)
        {
            if (documentId == null)
            {
                throw new ArgumentNullException(nameof(documentId));
            }

            Uri docUri = UriFactory.CreateDocumentUri(_dbName, _collectionName, documentId);
            await _client.DeleteDocumentAsync(docUri);
        }
    }
}
