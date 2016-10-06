using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Repsaj.Submerged.GatewayApp.Universal.Repositories
{
    public class StorageRepository : IStorageRepository
    {
        Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;

        public string GetStorageLocationPath()
        {
            return localFolder.Path;
        }

        public async Task<dynamic> GetStoredObject(string filename)
        {
            try
            {
                StorageFile file = await localFolder.GetFileAsync(filename);
                string fileContents = await FileIO.ReadTextAsync(file);
                dynamic result = JsonConvert.DeserializeObject(fileContents);
                return result;
            }
            catch (Exception ex)
            {
                // TODO: add logging
                return null;
            }
        }

        public async Task SaveObjectToStorage(dynamic obj, string filename)
        {
            string json = JsonConvert.SerializeObject(obj);
            StorageFile file = await localFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, json);
        }
    }
}
