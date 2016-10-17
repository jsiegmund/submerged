using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using Microsoft.Maker.Firmata;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using Windows.System.Threading;
using System.Threading;
using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Repsaj.Submerged.Gateway.Common.Log;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules
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

        internal readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);
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

                await Task.Run(() =>
                {
                    _adapter.begin(9600, SerialConfig.SERIAL_8N1);
                });
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error($"Initializing connection to module [{ModuleName}] failed: {ex}");
                SetModuleStatus(ModuleConnectionStatus.Disconnected);
            }
        }

        public bool IsReady()
        {
            return _adapter.connectionReady() && _firmata.connectionReady();
        }

        private void _adapter_ConnectionLost(string message)
        {
            LogEventSource.Log.Warn($"Bluetooth connection to [{ModuleName}] lost.");
            SetModuleStatus(ModuleConnectionStatus.Disconnected);
        }

        private void _adapter_ConnectionFailed(string message)
        {
            LogEventSource.Log.Error($"Bluetooth connection to [{ModuleName}] failed.");
            SetModuleStatus(ModuleConnectionStatus.Disconnected);
        }

        private void _adapter_ConnectionEstablished()
        {
            LogEventSource.Log.Info($"Bluetooth connection to [{ModuleName}] established.");
            _firmata.begin(_adapter);
        }

        private void _firmata_FirmataConnectionReady()
        {
            LogEventSource.Log.Info($"Firmata connection ready for action on module {ModuleName}.");
        }

        private void _firmata_FirmataConnectionLost(string message)
        {
            LogEventSource.Log.Warn($"Firmata connection lost on module {ModuleName}.");
            SetModuleStatus(ModuleConnectionStatus.Disconnected);
        }

        private void _firmata_FirmataConnectionFailed(string message)
        {
            LogEventSource.Log.Error($"Firmata connection failed on module {ModuleName}.");
            SetModuleStatus(ModuleConnectionStatus.Disconnected);
        }

        virtual internal void _arduino_DeviceReady()
        {
            LogEventSource.Log.Info($"Remote device connection for [{ModuleName}] ready for action!");
            SetModuleStatus(ModuleConnectionStatus.Connected);
        }

        private void _arduino_DeviceConnectionLost(string message)
        {
            LogEventSource.Log.Warn($"Remote device connection for [{ModuleName}] lost.");
            SetModuleStatus(ModuleConnectionStatus.Disconnected);
        }

        private void _arduino_DeviceConnectionFailed(string message)
        {
            LogEventSource.Log.Error($"Remote device connection for [{ModuleName}] failed.");
            SetModuleStatus(ModuleConnectionStatus.Disconnected);
        }

        protected void SetModuleStatus(ModuleConnectionStatus newStatus)
        {
            if (ModuleStatus != newStatus)
            {

                ModuleConnectionStatus oldStatus = ModuleStatus;
                ModuleStatus = newStatus;

                // ignore status Connecting because that would cause a lot of chatter
                ModuleStatusChanged?.Invoke(ModuleName, oldStatus, newStatus);
            }
        }

        public async Task Reconnect()
        {
            this._timer = null;

            _connectionLock.Wait();

            try
            {
                LogEventSource.Log.Info($"Reconnecting module {ModuleName} after a loss of connection.");
                SetModuleStatus(ModuleConnectionStatus.Connecting);

                await Task.Run(() =>
                {
                    try
                    {
                        if (_adapter.connectionReady())
                            _adapter.end();
                    }
                    catch
                    {
                        // ignore any exceptions
                    }

                    _adapter.begin(9600, SerialConfig.SERIAL_8N1);
                });
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error($"Error reconnecting to module {ModuleName}: {ex}.");
                SetModuleStatus(ModuleConnectionStatus.Disconnected);
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public void DisposeConnections()
        {
            // cleanly dispose the bluetooth connection
            if (_arduino != null)
            {
                _arduino.Dispose();
                _arduino = null;
            }
            if (_firmata != null)
            {
                _firmata.Dispose();
                _firmata = null;
            }
            if (_adapter != null)
            {
                _adapter.Dispose();
                _adapter = null;
            }
        }

        public void Dispose()
        {
            DisposeConnections();
        }
    }
}
