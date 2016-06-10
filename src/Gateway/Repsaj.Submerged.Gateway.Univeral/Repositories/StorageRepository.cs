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
        public async Task<dynamic> GetStoredObject(string filename)
        {
            try
            {
                Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
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
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            string json = JsonConvert.SerializeObject(obj);

            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            using (StreamWriter writeFile = new StreamWriter(await file.OpenStreamForWriteAsync()))
            {
                writeFile.WriteLine(json);
            }
        }
    }
}
