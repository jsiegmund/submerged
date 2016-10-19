using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Windows.Devices.Enumeration;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules
{
    abstract class GenericModuleConnectionBase : IModuleConnection
    {
        public event IModuleConnectionStatusChanged ModuleStatusChanged;

        private readonly object _statusLock = new object();

        private ModuleConnectionStatus _moduleStatus = ModuleConnectionStatus.Initializing;
        public ModuleConnectionStatus ModuleStatus
        {
            get { return _moduleStatus; }
            private set { _moduleStatus = value; }
        }

        public string ModuleName { get; private set; }
        public abstract string ModuleType { get; }

        public GenericModuleConnectionBase(string name)
        {
            this.ModuleName = name;
        }

        protected void SetModuleStatus(ModuleConnectionStatus newStatus)
        {
            if (ModuleStatus != newStatus)
            {

                lock (_statusLock)
                {
                    ModuleConnectionStatus oldStatus = ModuleStatus;
                    ModuleStatus = newStatus;

                    // ignore status Connecting because that would cause a lot of chatter
                    ModuleStatusChanged?.Invoke(ModuleName, oldStatus, newStatus);
                }
            }
        }

        public virtual Task Init()
        {
            // base class does not implement Init, but allow it to be overridden
            return Task.FromResult(0);
        }

        public virtual Task Reconnect()
        {
            // base class does not implement Init, but allow it to be overridden
            return Task.FromResult(0);
        }

        public virtual Task ProcessCommand(dynamic command)
        {
            // base class does not implement Init, but allow it to be overridden
            return Task.FromResult(0);
        }

        public virtual Task UpdateConfiguration(JObject configuration)
        {
            // base class does not implement Init, but allow it to be overridden
            return Task.FromResult(0);
        }

        public virtual void Dispose()
        {
            // base class does not implement Init, but allow it to be overridden
        }
    }
}
