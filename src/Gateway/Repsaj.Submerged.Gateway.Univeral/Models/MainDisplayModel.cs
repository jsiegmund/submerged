using Repsaj.Submerged.GatewayApp.Universal.Helpers;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Repsaj.Submerged.GatewayApp.Universal.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Models
{
    public class MainDisplayModel : NotificationBase
    {
        TrulyObservableCollection<ModuleDisplayModel> _modules = new TrulyObservableCollection<ModuleDisplayModel>();
        public TrulyObservableCollection<ModuleDisplayModel> Modules
        {
            get { return _modules; }
            set { SetProperty(ref _modules, value); }
        }

        TrulyObservableCollection<SensorDisplayModel> _sensors = new TrulyObservableCollection<SensorDisplayModel>();
        public TrulyObservableCollection<SensorDisplayModel> Sensors
        {
            get { return _sensors; }
            set { SetProperty(ref _sensors, value); }
        }

        TrulyObservableCollection<RelayDisplayModel> _relays = new TrulyObservableCollection<RelayDisplayModel>();
        public TrulyObservableCollection<RelayDisplayModel> Relays
        {
            get { return _relays; }
            set { SetProperty(ref _relays, value); }
        }

        public void ProcessSensorData(IEnumerable<Sensor> sensorData)
        {
            List<Sensor> sensorList = sensorData.ToList();

            // process all moisture sensors and remove the entries
            var moistureSensors = sensorList.Where(s => s.SensorType == SensorTypes.MOISTURE);
            ProcessLeakage(moistureSensors);
            sensorList.RemoveAll(s => s.SensorType == SensorTypes.MOISTURE);

            // process all other types of sensors
            foreach (var sensor in sensorList)
            {
                ProcessSensor(sensor);
            }

            RaisePropertyChanged(nameof(Sensors));
        }

        private void ProcessLeakage(IEnumerable<Sensor> sensor)
        {
            // all moisture sensors are grouped into one value that aggregates all of the values
            bool value = sensor.Any(s => (bool?)s.Reading == true);
            var sensorModel = this.Sensors.SingleOrDefault(s => s.Name == "Leakage");

            if (sensorModel == null)
            {
                SensorDisplayModel newSensor = new SensorDisplayModel()
                {
                    Name = "Leakage",
                    DisplayName = "Leak detection",
                    Reading = value,
                    SensorType = SensorTypes.MOISTURE,
                    OrderNumber = Int16.MaxValue
                };

                this.Sensors.Add(newSensor);
            }
            else
            {
                sensorModel.Reading = value;
            }
        }

        private void ProcessSensor(Sensor sensor)
        {
            var displayModel = this.Sensors.SingleOrDefault(s => s.Name == sensor.Name);

            if (displayModel == null && sensor.Reading != null)
            {
                displayModel = new SensorDisplayModel(sensor);                
                this.Sensors.Add(displayModel);
            }
            else if (displayModel != null && sensor.Reading == null)
            {
                this.Sensors.Remove(displayModel);
            }
            else if (displayModel != null && sensor.Reading != null)
            {
                displayModel.Reading = sensor.Reading;
                displayModel.Trend = sensor.Trend;
                displayModel.OrderNumber = sensor.OrderNumber;
            }

            this.Sensors.Sort(x => x.OrderNumber);
        }

        public void ProcessRelayData(IEnumerable<Relay> relayData)
        {
            foreach (var relay in relayData)
            {
                ProcessRelay(relay);
            }

            RaisePropertyChanged(nameof(Relays));
        }

        private void ProcessRelay(Relay relay)
        {
            var displayModel = this.Relays.SingleOrDefault(s => s.Name == relay.Name);

            if (displayModel == null)
            {
                displayModel = new RelayDisplayModel(relay);
                this.Relays.Add(displayModel);
            }
            else
            {
                displayModel.State = relay.State;
            }
        }

        public void ProcessModuleData(IEnumerable<Module> moduleData)
        {
            foreach (var module in moduleData)
            {
                ProcessModule(module);
            }

            RaisePropertyChanged(nameof(Modules));
        }

        private void ProcessModule(Module module)
        {
            var model = this.Modules.SingleOrDefault(s => s.Name == module.Name);

            if (model == null)
            {
                ModuleDisplayModel newModule = new ModuleDisplayModel(module);
                this.Modules.Add(newModule);
            }
            else
            {
                Debug.WriteLine($"Updated module {module.Name} to status {module.Status}");
                model.Status = module.Status;
            }
        }
    }
}
