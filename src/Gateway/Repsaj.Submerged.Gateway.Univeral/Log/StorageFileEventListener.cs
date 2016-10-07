using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Repsaj.Submerged.Gateway.Common.Log
{
    /// <summary> 
    /// This is an advanced usage, where you want to intercept the logging messages and devert them somewhere 
    /// besides ETW. 
    /// </summary> 
    public sealed class StorageFileEventListener : EventListener
    {
        Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;

        /// <summary> 
        /// Storage file to be used to write logs 
        /// </summary> 
        private StorageFile m_StorageFile = null;

        /// <summary> 
        /// Name of the current event listener 
        /// </summary> 
        private string m_Name;

        /// <summary> 
        /// The format to be used by logging. 
        /// </summary> 
        private string m_Format = "{0:yyyy-MM-dd HH\\:mm\\:ss\\:ffff}\tType: {1}\tId: {2}\tMessage: '{3}'";

        private string m_FileName;

        private SemaphoreSlim m_SemaphoreSlim = new SemaphoreSlim(1);

        public StorageFileEventListener(string name)
        {
            this.m_Name = name;
            this.m_FileName = m_Name.Replace(" ", "_") + ".log";

            Debug.WriteLine("StorageFileEventListener for {0} has name {1}", GetHashCode(), name);

            AssignLocalFile();
        }

        private async void AssignLocalFile()
        {
            await m_SemaphoreSlim.WaitAsync();

            try
            {
                m_StorageFile = await localFolder.CreateFileAsync(this.m_FileName, CreationCollisionOption.OpenIfExists);
            }
            finally
            {
                m_SemaphoreSlim.Release();
            }
        }

        private async void WriteToFile(IEnumerable<string> lines)
        {
            await m_SemaphoreSlim.WaitAsync();

            await Task.Run(async () =>
            {
                try
                {
                    await FileIO.AppendLinesAsync(m_StorageFile, lines);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Writing to the log file failed: " + ex.ToString());
                }
                finally
                {
                    m_SemaphoreSlim.Release();
                }
            });
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (m_StorageFile == null) return;

            var lines = new List<string>();
            var newFormatedLine = string.Format(m_Format, DateTime.Now, eventData.Level, eventData.EventId, eventData.Payload[0]);

            Debug.WriteLine(newFormatedLine);

            lines.Add(newFormatedLine);
            WriteToFile(lines);
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            Debug.WriteLine("OnEventSourceCreated for Listener {0} - {1} got eventSource {2}", GetHashCode(), m_Name, eventSource.Name);
        }
    }
}
