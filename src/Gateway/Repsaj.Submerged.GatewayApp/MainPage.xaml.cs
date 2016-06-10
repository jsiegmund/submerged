using Repsaj.Submerged.GatewayApp.Universal.Device;
using Microsoft.Azure.Devices.Client;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using System.Diagnostics;
using RemoteArduino.Commands;
using RemoteArduino.Hardware;
using Autofac;
using Windows.UI.Popups;
using Repsaj.Submerged.GatewayApp.Arduino;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Repositories;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Windows.System.Threading;
using Repsaj.Submerged.GatewayApp.Models;
using System.Text;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Repsaj.Submerged.GatewayApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IContainer _autofacContainer;
        ThreadPoolTimer timer;
        AzureConnection _azureConnection;
        ICommandProcessorFactory _commandProcessorFactory;
        IModuleConnectionManager _arduinoConnectionManager;
        ModuleConnectionFactory _moduleConnectionFactory;
        DeviceConfigurationModel _deviceConfig;

        int requestCounter = 0;
        Stack<string> logLines = new Stack<string>();

        public MainPage()
        {
            Application.Current.UnhandledException += Current_UnhandledException;
            Application.Current.Suspending += Current_Suspending;

            InitializeComponent();
            InitializeAutofac();

            //DeviceConfigurationModel configModel = new DeviceConfigurationModel()
            //{
            //    DeviceId = Statics.DeviceID,
            //    DeviceKey = Statics.DeviceKey,
            //    IoTHubHostname = Statics.IoTHostName
            //};

            // Resolve the local classes
            _commandProcessorFactory = _autofacContainer.Resolve<ICommandProcessorFactory>();
            _arduinoConnectionManager = _autofacContainer.Resolve<IModuleConnectionManager>();

            _arduinoConnectionManager.ModulesInitialized += _arduinoConnectionManager_ModulesInitialized;
            _arduinoConnectionManager.ModuleConnected += _arduinoConnectionManager_ModuleStatusChanged;
            _arduinoConnectionManager.ModuleDisconnected += _arduinoConnectionManager_ModuleStatusChanged;
            _arduinoConnectionManager.ModuleConnecting += _arduinoConnectionManager_ModuleStatusChanged;

            Task.Run(() => Init());
        }

        private async void _arduinoConnectionManager_ModulesInitialized()
        {
            await SendDeviceData();

            // when all modules have initialized; create a new timer 
            // and kick off the timer event once manually to immediately send the latest data
            timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, new TimeSpan(0, 0, 10));
            Timer_Tick(timer);
        }

        private void _arduinoConnectionManager_ModuleStatusChanged(string moduleName, bool notConnecting)
        {
            Task.Run(() => BindModules());
        }

        private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            //MinimalEventSource.Log.LogInfo("Suspending the application, killing the connection manager.");
            if (_arduinoConnectionManager != null)
            {
                _arduinoConnectionManager.Dispose();
                _arduinoConnectionManager = null;
            }
        }

        private async Task Init()
        {
            // First initialize the configuration
            await InitializeConfig();

            // Run the initialization in a seperate thread not to block the UI from loading
            Task _moduleTask = Task.Run(() => _arduinoConnectionManager.ConnectModules());
            Task _azureTask = Task.Run(() => InitializeAzure());
        }

        private async Task InitializeConfig()
        {
            IConfigurationRepository config = _autofacContainer.Resolve<IConfigurationRepository>();
            _deviceConfig = await config.GetDeviceConfiguration();
        }

        private async Task BindModules()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                listView_Modules.ItemsSource = _arduinoConnectionManager.GetModuleModels();
            });
        }

        private void InitializeAzure()
        {
            // Create an AzureConnection class to connect to Azure
            _azureConnection = new AzureConnection(_deviceConfig.IoTHubHostname, _deviceConfig.DeviceId, _deviceConfig.DeviceKey);
            _azureConnection.CommandReceived += _azureConnection_CommandReceived;
            Debug.WriteLine("Azure connection initialized.");
        }

        private void InitializeAutofac()
        {
            var builder = new ContainerBuilder();

            // register all implementations
            builder.RegisterType<StorageRepository>().As<IStorageRepository>();
            builder.RegisterType<ConfigurationRepository>().As<IConfigurationRepository>();
            builder.RegisterType<GPIOController>().As<IGPIOController>();
            builder.RegisterType<CommandProcessorFactory>().As<ICommandProcessorFactory>();
            builder.RegisterType<ModuleConnectionManager>().As<IModuleConnectionManager>();

            // register (singleton) instances
            _moduleConnectionFactory = new ModuleConnectionFactory();
            builder.RegisterInstance<ModuleConnectionFactory>(_moduleConnectionFactory);

            // register command processors
            builder.RegisterType<SwitchRelayCommandProcessor>().As<SwitchRelayCommandProcessor>();
            builder.RegisterType<DeviceInfoCommandProcessor>().As<DeviceInfoCommandProcessor>();

            var container = builder.Build();

            _autofacContainer = container;
        }

        private async Task _azureConnection_CommandReceived(DeserializableCommand command)
        {
            UpdateLog("Received cloud 2 device message: " + command);
            ICommandProcessor processor = _commandProcessorFactory.FindCommandProcessor(command);

            if (processor != null)
            {
                await processor.ProcessCommand(command);
                UpdateLog("Processed cloud 2 device message successfully.");
            }
        }

        private void Current_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // gracefully ignore all unhandled exceptions
            //MinimalEventSource.Log.LogError("Unhandled exception was caught: " + e.Exception.ToString());
        }
        
        private void _arduinoConnection_ConnectionFailed(string message)
        {
            UpdateLog(message);
        }

        private void Timer_Tick(ThreadPoolTimer timer)
        {
            RequestArduinoData();
        }

        private void RequestArduinoData()
        {
            JObject data = _arduinoConnectionManager.GetAvailableData();

            if (data != null)
            {
                JObject dataToBind = (JObject)data.DeepClone();
                Task.Run(() => BindSensorData(dataToBind));

                // we want to send new telemetry data every 6 requests ( = once a minute)
                requestCounter = (requestCounter + 1) % 6;
                if (requestCounter == 0)
                    Task.Run(() => SendTelemetryAsync(data));
            }
        }

        private async Task BindSensorData(JObject data)
        {
            List<SensorModel> result = new List<SensorModel>();
            foreach (var sensorData in data)
            {
                JToken valueToken = sensorData.Value;
                JValue value = (JValue)valueToken.Value<object>();
                result.Add(new SensorModel() { Name = sensorData.Key, Reading = value.Value });
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                listView_Sensors.ItemsSource = result;
            });
         }

        private async Task SendTelemetryAsync(JObject data)
        {
            try
            {
                TelemetryHelper.MakeTelemetryObject(data, _deviceConfig.DeviceId);

                // push the data to Azure for cloud processing
                string message = await _azureConnection.SendDeviceToCloudMessagesAsync(data);

                UpdateLog(String.Format("Telemetry sent @ {0:G}", DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                //MinimalEventSource.Log.LogError($"Failure trying to send the data to Azure: {ex}");
                UpdateLog(String.Format("Failure trying to send the data to Azure."));
            }
        }

        private async Task SendDeviceData()
        {
            Dictionary<string, string> statuses = _arduinoConnectionManager.GetModuleStatuses();
            dynamic data = DeviceFactory.GetDevice(_deviceConfig.DeviceId, _deviceConfig.DeviceKey, statuses);

            try
            {
                // push the data to Azure for cloud processing
                await _azureConnection.SendDeviceToCloudMessagesAsync(data);

                UpdateLog(String.Format("Device data sent @ {0:G}", DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                //MinimalEventSource.Log.LogError($"Failure trying to send device data to Azure: {ex}");
                UpdateLog(ex.ToString());
            }
        }

        private async void UpdateLog(string text)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                logLines.Push(text);

                while (logLines.Count > 10)
                    logLines.Pop();

                StringBuilder content = new StringBuilder();
                foreach (string line in logLines)
                    content.AppendLine(line);

                tbLog.Text = content.ToString();
            });
        }

        private async void btnExit_Click(object sender, RoutedEventArgs e)
        {
            // display a pop-up asking the user whether he's really really sure about leaving this awesome app
            MessageDialog dialog = new MessageDialog("Are you sure you want to exit? This will stop measurements being sent.");
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = 0 });
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("No") { Id = 1 });
            dialog.DefaultCommandIndex = 1;
            dialog.CancelCommandIndex = 1;
            var result = await dialog.ShowAsync();

            // exit the application when the result says the user chose 'Yes' ( Id = 0 )
            int? resultId = result.Id as int?;
            if (resultId.HasValue && resultId == 0)
                Application.Current.Exit();
        }

    }
}
