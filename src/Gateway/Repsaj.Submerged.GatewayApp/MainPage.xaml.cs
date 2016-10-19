using Repsaj.Submerged.GatewayApp.Universal.Device;
using Microsoft.Azure.Devices.Client;
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
using Autofac;
using Windows.UI.Popups;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Repositories;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Windows.System.Threading;
using System.Text;
using Repsaj.Submerged.GatewayApp.Device;
using Autofac.Core;
using System.Threading;
using Windows.UI.Xaml.Media.Imaging;
using Repsaj.Submerged.GatewayApp.Universal.Modules;
using Repsaj.Submerged.GatewayApp.Universal.Exceptions;
using System.Diagnostics.Tracing;
using Repsaj.Submerged.Gateway.Common.Log;
using Windows.ApplicationModel.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Repsaj.Submerged.GatewayApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //private IContainer _autofacContainer;
        private IConfigurationRepository _configRepository;
        IDeviceManager _deviceManager;

        public MainPage()
        {
            CoreApplication.UnhandledErrorDetected += CoreApplication_UnhandledErrorDetected;
            Application.Current.UnhandledException += Current_UnhandledException;
            Application.Current.Suspending += Current_Suspending;

            // initialize logging
            EventListener listener = new StorageFileEventListener("SubmergedListener");
            listener.EnableEvents(LogEventSource.Log, EventLevel.Warning);

            LogEventSource.Log.Info("The application is booting.");

            InitializeComponent();
            Autofac.InitializeAutofac();

            this.DataModel = new MainDisplayModel();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // run the initialization code in a new background task
                await Init();
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error($"Error during initialization of the Gateway: {ex}");
            }
        }

        public MainDisplayModel DataModel { get; set; }

        private async Task Init()
        {
            // resolve the config repository for saving and storing the configuration
            _configRepository = Autofac.Container.Resolve<IConfigurationRepository>();
            var connectionInfo = await _configRepository.GetConnectionInformationModelAsync();

            // set the instruction text
            string storagePath = _configRepository.GetConnectionInfoPath();
            string instruction = "STOP! Setup time. Your device was not configured to connect to the Submerged back-end. Please read the documentation " +
                                 $"and upload a valid json configuration file with connection parameters. The storage path is: {storagePath}, " +
                                 "Restart the submerged app and this message should disappear!";
            connectionInfoBox.Text = instruction;

            // upon first boot; the configuration will not have been set yet
            if (connectionInfo == null)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    messageBox.Visibility = Visibility.Visible;
                });
            }
            else
            {
                _deviceManager = Autofac.Container.Resolve<IDeviceManager>();
                _deviceManager.NewLogLine += UpdateLog;

                _deviceManager.ModulesUpdated += _deviceManager_ModulesUpdated;
                _deviceManager.SensorsUpdated += _deviceManager_SensorsUpdated;
                _deviceManager.RelaysUpdated += _deviceManager_RelaysUpdated;

                _deviceManager.AzureConnected += _deviceManager_AzureConnected;
                _deviceManager.AzureDisconnected += _deviceManager_AzureDisconnected;

                try
                {
                    await _deviceManager.Init();
                }
                catch (DeviceNotFoundException)
                {
                    instruction = "Uh-oh! It seems your device configuration is incorrect, or your device was not properly registered. Check the " +
                                  "configuration below and ensure the device ID + key are set correctly.";
                    ShowConnectionInfoBox(instruction, connectionInfo);
                }
                catch (Exception ex) when (ex is FormatException || ex is DeviceNotAuthorizedException)
                {
                    instruction = "Uh-oh! The device key you've entered is not a correct one. You should double check the key and try again.";
                    ShowConnectionInfoBox(instruction, connectionInfo);
                }
                catch (Exception ex)
                {
                    LogEventSource.Log.Error("Could not initialize due to this exception: " + ex.ToString());

                    instruction = "Uh-oh! We could not connect to the submerged back-end. Your internet connection might have an issue, in rare " +
                              "cases the problem could be at the back-end or maybe it's very clouded. Check the details below to ensure your " +
                              "connection is set-up ok.";
                    ShowConnectionInfoBox(instruction, connectionInfo);
                }
            }
        }

        private void ShowConnectionInfoBox(string text, ConnectionInformationModel connectionInfo)
        {
            connectionInfoBox.Text = text;

            if (connectionInfo != null)
            {
                tbDeviceID.Text = connectionInfo.DeviceId;
                tbDeviceKey.Text = connectionInfo.DeviceKey;
            }

            messageBox.Visibility = Visibility.Visible;
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
            LogEventSource.Log.Info("Suspending the application, killing the connection manager.");

            if (_deviceManager != null)
            {
                _deviceManager.Dispose();
                _deviceManager = null;
            }
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

        private async void SetupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // save a new connection.json file using the device ID and key from the user input
                ConnectionInformationModel connectionInfo = new ConnectionInformationModel()
                {
                    DeviceId = tbDeviceID.Text,
                    DeviceKey = tbDeviceKey.Text,
                    IoTHubHostname = "repsaj-neptune-iothub.azure-devices.net"      // TODO: hardcoded for now, should probably be moved somewhere else?
                };

                // save the configuration data to file
                await _configRepository.SaveConnectionInformationModelAsync(connectionInfo);

                // start initialization
                await Init();

                // hide the messagebox
                messageBox.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error("Failure saving settings: " + ex.ToString());
            }
        }
        private async void Current_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // gracefully ignore all unhandled exceptions
            await Task.Run(() => { LogEventSource.Log.Error("Unhandled exception crashed the app: " + e.ToString()); });
        }

        private async void CoreApplication_UnhandledErrorDetected(object sender, UnhandledErrorDetectedEventArgs e)
        {
            await Task.Run(() => { LogEventSource.Log.Error("Unhandled exception crashed the app: " + e.ToString()); });
        }

    }
}