using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules.Simulated
{
    public abstract class SimulatedModuleConnectionBase: IModuleConnection
    {
        public event IModuleConnectionStatusChanged ModuleStatusChanged;

        public string ModuleName { get; private set; }
        public abstract string ModuleType { get; }

        private ModuleConnectionStatus _moduleStatus = ModuleConnectionStatus.Initializing;
        public ModuleConnectionStatus ModuleStatus
        {
            get { return _moduleStatus; }
            private set { _moduleStatus = value; }
        }

        public abstract void SwitchRelay(string name, bool high);

        public string StatusAsText
        {
            get { return Enum.GetName(typeof(ModuleConnectionStatus), this.ModuleStatus); }
        }

        public SimulatedModuleConnectionBase(string moduleName)
        {
            this.ModuleName = moduleName;
        }

        public void Init()
        {
            // does nothing because this is a simulated module
            SetConnectedDelayed().Wait();
        }

        public void Reconnect()
        {
            // does nothing because this is a simulated module
            SetConnectedDelayed().Wait();
        }

        private async Task SetConnectedDelayed()
        {
            // simulate a delay before the module is actually connected
            await Task.Delay(TimeSpan.FromSeconds(Randomizer.GetRandomInt(5,3)));
            SetModuleStatus(ModuleConnectionStatus.Connected);
        }

        public void Dispose()
        {
            // does nothing because this is a simulated module
        }

        protected void SetModuleStatus(ModuleConnectionStatus newStatus)
        {
            if (ModuleStatus != newStatus)
            {

                ModuleConnectionStatus oldStatus = ModuleStatus;
                ModuleStatus = newStatus;
                Debug.WriteLine($"Module [{ModuleName}] changed its status to: {StatusAsText}");

                // ignore status Connecting because that would cause a lot of chatter
                ModuleStatusChanged?.Invoke(ModuleName, oldStatus, newStatus);
            }
        }
    }
}
