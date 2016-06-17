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
using Repsaj.Submerged.GatewayApp.Device;
using Autofac.Core;
using System.Threading;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Repsaj.Submerged.GatewayApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IContainer _autofacContainer;
        IDeviceManager _deviceManager;

        Stack<string> logLines = new Stack<string>();

        public MainPage()
        {
            Application.Current.UnhandledException += Current_UnhandledException;
            Application.Current.Suspending += Current_Suspending;

            InitializeComponent();
            InitializeAutofac();

            Submerged = new MainModel();


            ConnectionInformationModel model = new ConnectionInformationModel()
            {
                IoTHubHostname = "repsaj-neptune-iothub.azure-devices.net",
                DeviceId = "repsaj-neptune-win10pi",
                DeviceKey = "p+fkLZ1Vc+uB0YW6YjjE6PZ7KA2bshLRaL9rl4tfajQ=",
            };
            IConfigurationRepository configRepository = _autofacContainer.Resolve<IConfigurationRepository>();
            configRepository.SaveConnectionInformationModel(model);

            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        public MainModel Submerged{ get; set; }

        private async void Init()
        {
            IConfigurationRepository configRepository = _autofacContainer.Resolve<IConfigurationRepository>();
            var configInfo = await configRepository.GetConnectionInformationModel();

            // upon first boot; the configuration will not have been set yet
            if (configInfo == null)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    string instruction = "Your device was not configured to connect to the Submerged back-end. Please read the documentation " +
                                         "and upload a valid json configuration file with connection parameters. Restart the submerged app and " +
                                         "this message should disappear!";
                    messageBoxText.Text = instruction;
                    messageBox.Visibility = Visibility.Visible;
                });
            }
            else
            {
                _deviceManager = _autofacContainer.Resolve<IDeviceManager>();
                _deviceManager.NewLogLine += UpdateLog;
                _deviceManager.SensorDataChanged += _deviceManager_SensorDataChanged;
                _deviceManager.ModuleDataChanged += _deviceManager_ModuleDataChanged;
                _deviceManager.RelayDataChanged += _deviceManager_RelayDataChanged;
                _deviceManager.AzureConnected += _deviceManager_AzureConnected;
                _deviceManager.AzureDisconnected += _deviceManager_AzureDisconnected;
                await _deviceManager.Init();
            }
        }

        private async void _deviceManager_AzureDisconnected()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this._imgCloudLogo.Source = new BitmapImage(new Uri("ms-appx:///Icons/Cloud-Download-48.png", UriKind.Absolute));
            });
        }

        private async void _deviceManager_AzureConnected()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this._imgCloudLogo.Source = new BitmapImage(new Uri("ms-appx:///Icons/Cloud-Upload-48.png", UriKind.Absolute));
            });
        }

        private async void _deviceManager_RelayDataChanged(IEnumerable<Relay> relays)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (var relay in relays)
                {
                    var model = Submerged.Relays.SingleOrDefault(s => s.Name == relay.Name);

                    if (model == null)
                    {
                        Submerged.Relays.Add(new RelayModel(relay));
                    }
                    else
                    {
                        model.State = relay.State;
                    }
                }
            });
        }

        public async void _deviceManager_ModuleDataChanged(IEnumerable<Universal.Models.Module> modules)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (var module in modules)
                {
                    var model = Submerged.Modules.SingleOrDefault(s => s.Name == module.Name);

                    if (model == null)
                    {
                        Submerged.Modules.Add(new ModuleModel(module));
                    }
                    else
                    {
                        model.Status = module.Status;
                    }
                }
            });
        }

        private async void _deviceManager_SensorDataChanged(IEnumerable<Sensor> sensors)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (var sensor in sensors)
                {
                    var model = Submerged.Sensors.SingleOrDefault(s => s.Name == sensor.Name);

                    if (model == null)
                    {
                        Submerged.Sensors.Add(new SensorModel(sensor));
                    }
                    else
                    {
                        model.Reading = sensor.Reading;
                    }
                }
            });
        }

        private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            //MinimalEventSource.Log.LogInfo("Suspending the application, killing the connection manager.");
            if (_deviceManager != null)
            {
                _deviceManager.Dispose();
                _deviceManager = null;
            }
        }

        private void Current_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // gracefully ignore all unhandled exceptions
            //MinimalEventSource.Log.LogError("Unhandled exception was caught: " + e.Exception.ToString());
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
            builder.RegisterType<ModuleConnectionFactory>().As<IModuleConnectionFactory>();

            builder.RegisterType<DeviceManager>().As<IDeviceManager>()
                   .WithParameter(new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(SynchronizationContext),
                        (pi, ctx) => SynchronizationContext.Current));

            // register command processors
            builder.RegisterType<SwitchRelayCommandProcessor>().As<SwitchRelayCommandProcessor>();
            builder.RegisterType<DeviceInfoCommandProcessor>().As<DeviceInfoCommandProcessor>();

            var container = builder.Build();
            _autofacContainer = container;
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

    }
}
