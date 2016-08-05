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
using Repsaj.Submerged.GatewayApp.Modules;

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

        public MainPage()
        {
            Application.Current.UnhandledException += Current_UnhandledException;
            Application.Current.Suspending += Current_Suspending;

            InitializeComponent();
            InitializeAutofac();

            this.DataModel = new MainDisplayModel();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        public MainDisplayModel DataModel{ get; set; }
         
        private async void Init()
        {
            IConfigurationRepository configRepository = _autofacContainer.Resolve<IConfigurationRepository>();
            var configInfo = await configRepository.GetConnectionInformationModel();

            // upon first boot; the configuration will not have been set yet
            if (configInfo == null)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    string storagePath = configRepository.GetConnectionInfoPath();

                    string instruction = "Your device was not configured to connect to the Submerged back-end. Please read the documentation " +
                                         $"and upload a valid json configuration file with connection parameters. The storage path is: {storagePath}, " +
                                         "Restart the submerged app and this message should disappear!";
                    messageBoxText.Text = instruction;
                    messageBox.Visibility = Visibility.Visible;
                });
            }
            else
            {
                _deviceManager = _autofacContainer.Resolve<IDeviceManager>();
                _deviceManager.NewLogLine += UpdateLog;

                _deviceManager.ModulesUpdated += _deviceManager_ModulesUpdated;
                _deviceManager.SensorsUpdated += _deviceManager_SensorsUpdated;
                _deviceManager.RelaysUpdated += _deviceManager_RelaysUpdated;

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

        public async void _deviceManager_ModulesUpdated(IEnumerable<Universal.Models.Module> modules)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                DataModel.ProcessModuleData(modules);
            });
        }

        private async void _deviceManager_SensorsUpdated(IEnumerable<Sensor> sensors)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                DataModel.ProcessSensorData(sensors);
            });
        }

        private async void _deviceManager_RelaysUpdated(IEnumerable<Relay> relays)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                DataModel.ProcessRelayData(relays);
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

            // register single instances
            builder.RegisterType<GPIOController>().As<IGPIOController>().SingleInstance();
            builder.RegisterType<CommandProcessorFactory>().As<ICommandProcessorFactory>().SingleInstance();
            builder.RegisterType<ModuleConnectionManager>().As<IModuleConnectionManager>().SingleInstance();
            builder.RegisterType<ModuleConnectionFactory>().As<IModuleConnectionFactory>().SingleInstance();

            builder.RegisterType<DeviceManager>().As<IDeviceManager>().SingleInstance()
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
                string[] lines = tbLog.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                List<string> linesList = new List<string>(lines);

                // remove old lines when the count reaches 100 
                while (linesList.Count > 100)
                    linesList.RemoveAt(linesList.Count - 1);

                // insert the new line at the top of the list
                linesList.Insert(0, text);

                tbLog.Text = string.Join(Environment.NewLine, linesList);
            });
        }

    }
}
