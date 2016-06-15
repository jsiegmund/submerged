using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Repsaj.Submerged.GatewayApp.Models
{
    public class SensorModel : NotificationBase<Sensor>
    {
        public SensorModel(Sensor sensor = null) : base(sensor)
        {
            if (This.SensorType == SensorTypes.TEMPERATURE)
            {
                var value = "ms-appx:///Icons/Sensor_Temperature.png";
                SetProperty(this._iconImageUrl, value, () => this._iconImageUrl = value);
            }
            else
            {
                var value = "ms-appx:///Icons/Sensor_Gauge.png";
                SetProperty(this._iconImageUrl, value, () => this._iconImageUrl = value);
            }
        }

        public String Name
        {
            get { return This.Name; }
            set { SetProperty(This.Name, value, () => This.Name = value); }
        }

        public String DisplayName
        {
            get { return This.DisplayName; }
            set { SetProperty(This.DisplayName, value, () => This.DisplayName = value); }
        }

        private object _reading;
        public object Reading
        {
            get { return _reading; }
            set { SetProperty(this._reading, value, () => this._reading = value); RaisePropertyChanged("ReadingAsText"); }
        }

        private string _iconImageUrl;
        public string IconImageUri
        {
            get { return _iconImageUrl; }
        }

        public string ReadingAsText
        {
            get
            {
                string formattedText = "";

                if (Reading is double || Reading is float)
                {
                    if (This.SensorType == SensorTypes.TEMPERATURE)
                        formattedText = String.Format("{0:0.0}", Reading);
                    if (This.SensorType == SensorTypes.PH)
                        formattedText = String.Format("{0:0.00}", Reading);
                    else
                        formattedText = String.Format("{0:0.0}", Reading);
                }
                else
                    formattedText = Reading?.ToString();

                return formattedText;
            }
        }
    }
}
