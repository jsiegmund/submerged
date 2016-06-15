using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Models
{
    public class MainModel : NotificationBase
    {
        ObservableCollection<ModuleModel> _modules = new ObservableCollection<ModuleModel>();
        public ObservableCollection<ModuleModel> Modules
        {
            get { return _modules; }
            set { SetProperty(ref _modules, value); }
        }

        ObservableCollection<SensorModel> _sensors = new ObservableCollection<SensorModel>();
        public ObservableCollection<SensorModel> Sensors
        {
            get { return _sensors; }
            set { SetProperty(ref _sensors, value); }
        }

        ObservableCollection<RelayModel> _relays = new ObservableCollection<RelayModel>();
        public ObservableCollection<RelayModel> Relays
        {
            get { return _relays; }
            set { SetProperty(ref _relays, value); }
        }
    }
}
