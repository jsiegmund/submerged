using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Modules.Simulated
{
    public abstract class SimulatedModuleConnectionBase: IModuleConnection
    {
        public event IModuleConnectionStatusChanged ModuleStatusChanged;

        public string ModuleName { get; private set; }
        public abstract string ModuleType { get; }
        public ModuleConnectionStatus ModuleStatus { get; private set; }
        public abstract dynamic RequestArduinoData();

        public string StatusAsText
        {
            get { return Enum.GetName(typeof(ModuleConnectionStatus), this.ModuleStatus); }
        }

        public SimulatedModuleConnectionBase(string moduleName)
        {
            this.ModuleName = moduleName;
            this.ModuleStatus = ModuleConnectionStatus.Initializing;
        }

        public async void Init()
        {
            // does nothing because this is a simulated module
            await SetConnectedDelayed();
        }

        private async Task SetConnectedDelayed()
        {
            // simulate a delay before the module is actually connected
            await Task.Delay(TimeSpan.FromSeconds(5));
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
