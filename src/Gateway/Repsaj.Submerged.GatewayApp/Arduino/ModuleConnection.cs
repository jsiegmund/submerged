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

namespace Repsaj.Submerged.GatewayApp.Arduino
{
    public abstract class ModuleConnection : IDisposable
    {
        public delegate void IModuleStatusChanged(string moduleName, ModuleConnectionStatus oldStatus, ModuleConnectionStatus newStatus);
        public event IModuleStatusChanged ModuleStatusChanged;

        internal DeviceInformation _device;
        internal RemoteDevice _arduino;
        internal BluetoothSerial _adapter;
        internal UwpFirmata _firmata;

        abstract internal void _firmata_StringMessageReceived(UwpFirmata caller, StringCallbackEventArgs argv);
        abstract internal dynamic RequestArduinoData();

        public string ModuleName { get; private set; }
        public abstract string ModuleType { get; }
        public ModuleConnectionStatus ModuleStatus { get; private set;}

        EventWaitHandle _waitHandle = new AutoResetEvent(true);
        private readonly object _statusLock = new object();
        private readonly object _connectionLock = new object();
        protected object ConnectionLock
        {
            get { return _connectionLock; }
        }        

        private ThreadPoolTimer _timer;

        public ModuleConnection(DeviceInformation device, string name)
        {
            this._device = device;
            this.ModuleName = name;
        }

        internal string StatusAsText
        {
            get { return Enum.GetName(typeof(ModuleConnectionStatus), this.ModuleStatus); }
        }

        private dynamic _deviceProperties;
        public dynamic DeviceProperties
        {
            get { return _deviceProperties; }
            set { _deviceProperties = value; }
        }


        public void Init()
        {
            Debug.WriteLine($"Initializing connection for module [{ModuleName}]");
            SetModuleStatus(ModuleConnectionStatus.Initializing);

            try
            {
                lock (ConnectionLock)
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

                    _adapter.begin(0, SerialConfig.SERIAL_8N1);
                    _waitHandle.WaitOne();

                    _firmata.begin(_adapter);
                    _waitHandle.WaitOne();
                }
            }
            catch (Exception ex)
            {
                //MinimalEventSource.Log.LogError($"Initializing connection to module [{ModuleName}] failed: {ex}");
                ResetConnection();
            }
        }

        public bool IsReady()
        {
            return _adapter.connectionReady() && _firmata.connectionReady();
        }

        private void _adapter_ConnectionLost(string message)
        {
            //MinimalEventSource.Log.LogWarning($"Bluetooth connection to [{ModuleName}] lost.");
            SetModuleStatus(ModuleConnectionStatus.Disconnected);
        }

        private void _adapter_ConnectionFailed(string message)
        {
            //MinimalEventSource.Log.LogError($"Bluetooth connection to [{ModuleName}] failed.");
            _waitHandle.Set();
        }

        private void _adapter_ConnectionEstablished()
        {
            //MinimalEventSource.Log.LogInfo($"Bluetooth connection to [{ModuleName}] established.");
            _waitHandle.Set();
        }

        private void _firmata_FirmataConnectionReady()
        {
            //MinimalEventSource.Log.LogInfo($"Firmata connection ready for action on module {ModuleName}.");
            _waitHandle.Set();
        }

        private void _firmata_FirmataConnectionLost(string message)
        {
            //MinimalEventSource.Log.LogWarning($"Firmata connection lost on module {ModuleName}.");
            SetModuleStatus(ModuleConnectionStatus.Disconnected);
        }

        private void _firmata_FirmataConnectionFailed(string message)
        {
            //MinimalEventSource.Log.LogError($"Firmata connection failed on module {ModuleName}.");
            SetModuleStatus(ModuleConnectionStatus.Disconnected);
            _waitHandle.Set();
        }

        virtual internal void _arduino_DeviceReady()
        {
            //MinimalEventSource.Log.LogInfo($"Remote device connection for [{ModuleName}] ready for action!");
            SetModuleStatus(ModuleConnectionStatus.Connected);
        }

        private void _arduino_DeviceConnectionLost(string message)
        {
            //MinimalEventSource.Log.LogWarning($"Remote device connection for [{ModuleName}] lost.");
            SetModuleStatus(ModuleConnectionStatus.Disconnected);
        }

        private void _arduino_DeviceConnectionFailed(string message)
        {
            //MinimalEventSource.Log.LogError($"Remote device connection for [{ModuleName}] failed.");
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
                    Debug.WriteLine($"Module [{ModuleName}] changed its status to: {StatusAsText}");

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
                    _waitHandle.WaitOne();
                }

                if (!_firmata.connectionReady())
                {
                    _firmata.begin(_adapter);
                    _waitHandle.WaitOne();
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
