using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using Microsoft.Maker.Firmata;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using Windows.System.Threading;
using System.Threading;
using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Models;
using Repsaj.Submerged.Gateway.Common.Log;

namespace Repsaj.Submerged.GatewayApp.Modules
{ 
    abstract class FirmataModuleConnectionBase : IDisposable, IModuleConnection
    {
        public event IModuleConnectionStatusChanged ModuleStatusChanged;

        internal DeviceInformation _device;
        internal RemoteDevice _arduino;
        internal BluetoothSerial _adapter;
        internal UwpFirmata _firmata;

        internal abstract void _firmata_StringMessageReceived(UwpFirmata caller, StringCallbackEventArgs argv);

        public string ModuleName { get; private set; }
        public abstract string ModuleType { get; }

        private ModuleConnectionStatus _moduleStatus = ModuleConnectionStatus.Initializing;
        public ModuleConnectionStatus ModuleStatus
        {
            get { return _moduleStatus; }
            private set { _moduleStatus = value; }
        }

        private readonly object _statusLock = new object();
        private readonly object _connectionLock = new object();
        protected object ConnectionLock
        {
            get { return _connectionLock; }
        }        

        private ThreadPoolTimer _timer;

        public FirmataModuleConnectionBase(DeviceInformation device, string name)
        {
            this._device = device;
            this.ModuleName = name;
        }

        private dynamic _deviceProperties;
        public dynamic DeviceProperties
        {
            get { return _deviceProperties; }
            set { _deviceProperties = value; }
        }


        public async Task Init()
        {
            // when no valid device info was passed in, this module will remain idle
            if (_device == null)
            {
                SetModuleStatus(ModuleConnectionStatus.NotRegistered);
                return;
            }

            Debug.WriteLine($"Initializing connection for module [{ModuleName}]");

            try
            {
                //DeviceInformation deviceInfo = await DeviceInformation.CreateFromIdAsync(BluetoothModuleId);
                _adapter = new BluetoothSerial(_device);
                _adapter.ConnectionEstablished += _adapter_ConnectionEstablished;
                _adapter.ConnectionFailed += _adapter_ConnectionFailed;
                _adapter.ConnectionLost += _adapter_ConnectionLost;

                _firmata = new UwpFirmata();
                _firmata.FirmataConnectionFailed += _firmata_FirmataConnectionFailed;
                _firmata.FirmataConnectionLost += _firmata_FirmataConnectionLost;
                _firmata.FirmataConnectionReady += _firmata_FirmataConnectionReady;
                _firmata.StringMessageReceived += _firmata_StringMessageReceived;

                _arduino = new RemoteDevice(_firmata);
                _arduino.DeviceConnectionFailed += _arduino_DeviceConnectionFailed;
                _arduino.DeviceConnectionLost += _arduino_DeviceConnectionLost;
                _arduino.DeviceReady += _arduino_DeviceReady;

                _adapter.begin(9600, SerialConfig.SERIAL_8N1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Initializing connection to module [{ModuleName}] failed: {ex}");
                LogEventSource.Log.Error($"Initializing connection to module [{ModuleName}] failed: {ex}");
                ResetConnection();
            }
        }

        public bool IsReady()
        {
            return _adapter.connectionReady() && _firmata.connectionReady();
        }

        private void _adapter_ConnectionLost(string message)
        {
            Debug.WriteLine($"Module {this.ModuleName} lost BluetoothSerial connection.");
            LogEventSource.Log.Warn($"Bluetooth connection to [{ModuleName}] lost.");
            SetModuleStatus(ModuleConnectionStatus.Disconnected);
        }

        private void _adapter_ConnectionFailed(string message)
        {
            Debug.WriteLine($"Module {this.ModuleName} could not create BluetoothSerial connection.");
            LogEventSource.Log.Error($"Bluetooth connection to [{ModuleName}] failed.");
            SetModuleStatus(ModuleConnectionStatus.Disconnected);
        }

        private void _adapter_ConnectionEstablished()
        {
            Debug.WriteLine($"Bluetooth connection to [{ModuleName}] established.");
            LogEventSource.Log.Info($"Bluetooth connection to [{ModuleName}] established.");
            _firmata.begin(_adapter);
        }

        private void _firmata_FirmataConnectionReady()
        {
            Debug.WriteLine($"Firmata connection ready for action on module {ModuleName}.");
            LogEventSource.Log.Info($"Firmata connection ready for action on module {ModuleName}.");
        }

        private void _firmata_FirmataConnectionLost(string message)
        {
            Debug.WriteLine($"Module {this.ModuleName} lost Firmata connection.");
            LogEventSource.Log.Warn($"Firmata connection lost on module {ModuleName}.");
            SetModuleStatus(ModuleConnectionStatus.Disconnected);
        }

        private void _firmata_FirmataConnectionFailed(string message)
        {
            Debug.WriteLine($"Module {this.ModuleName} could not create Firmata connection.");
            LogEventSource.Log.Error($"Firmata connection failed on module {ModuleName}.");
            SetModuleStatus(ModuleConnectionStatus.Disconnected);
        }

        virtual internal void _arduino_DeviceReady()
        {
            Debug.WriteLine($"Remote device connection for [{ModuleName}] ready for action!");
            LogEventSource.Log.Info($"Remote device connection for [{ModuleName}] ready for action!");
            SetModuleStatus(ModuleConnectionStatus.Connected);
        }

        private void _arduino_DeviceConnectionLost(string message)
        {
            Debug.WriteLine($"Module {this.ModuleName} lost RemoteDevice connection.");
            LogEventSource.Log.Warn($"Remote device connection for [{ModuleName}] lost.");
            SetModuleStatus(ModuleConnectionStatus.Disconnected);
        }

        private void _arduino_DeviceConnectionFailed(string message)
        {
            Debug.WriteLine($"Module {this.ModuleName} could not create RemoteDevice connection.");
            LogEventSource.Log.Error($"Remote device connection for [{ModuleName}] failed.");
            SetModuleStatus(ModuleConnectionStatus.Disconnected);
        }

        protected void SetModuleStatus(ModuleConnectionStatus newStatus)
        {
            if (ModuleStatus != newStatus)
            {

                lock (_statusLock)
                {
                    ModuleConnectionStatus oldStatus = ModuleStatus;
                    ModuleStatus = newStatus;

                    // if the new status equals Disconnected, start a timer to reconnect
                    if (newStatus == ModuleConnectionStatus.Disconnected)
                        ResetConnection();

                    // ignore status Connecting because that would cause a lot of chatter
                    ModuleStatusChanged?.Invoke(ModuleName, oldStatus, newStatus);
                }
            }
        }

        private void ResetConnection()
        {
            if (_timer == null)
            {
                Debug.WriteLine($"Creating timer object to reconnect module [{ModuleName}] in 5 minutes.");
                _timer = ThreadPoolTimer.CreateTimer(DoReset, new TimeSpan(0, 5, 0));
            }
        }

        private void DoReset(ThreadPoolTimer timer)
        {
            this._timer = null;

            lock (ConnectionLock)
            {
                //MinimalEventSource.Log.LogInfo($"Reconnecting module {ModuleName} after a loss of connection.");
                SetModuleStatus(ModuleConnectionStatus.Connecting);

                if (!_adapter.connectionReady())
                {
                    _adapter.begin(0, SerialConfig.SERIAL_8N1);
                }
                else if (!_firmata.connectionReady())
                {
                    _firmata.begin(_adapter);
                }
            }
        }

        public void Dispose()
        {
            // cleanly dispose the bluetooth connection
            if (_firmata != null)
            {
                _firmata.finish();
                _firmata.Dispose();
                _firmata = null;
            }

            if (_adapter != null)
            {
                _adapter.end();
                _adapter.Dispose();
                _adapter = null;
            }
        }
    }
}
