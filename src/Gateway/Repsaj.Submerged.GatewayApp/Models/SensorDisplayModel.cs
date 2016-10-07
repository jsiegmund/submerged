using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Helpers;
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
    public class SensorDisplayModel : NotificationBase
    {
        public SensorDisplayModel() { }

        public SensorDisplayModel(Sensor sensor) : base()
        {
            this._name = sensor.Name;
            this._displayName = sensor.DisplayName;
            this._reading = sensor.Reading;
            this._sensorType = sensor.SensorType;
            this._orderNumber = sensor.OrderNumber;
        }

        string _name;
        public String Name
        {
            get { return this._name; }
            set { SetProperty(this._name, value, () => this._name = value); }
        }

        string _displayName;
        public String DisplayName
        {
            get { return this._displayName; }
            set { SetProperty(this._displayName, value, () => this._displayName = value); }
        }

        string _sensorType;
        public String SensorType
        {
            get { return this._sensorType; }
            set
            {
                SetProperty(this._sensorType, value, () => this._sensorType = value);
                RaisePropertyChanged(nameof(IconImageUri));
            }
        }

        object _reading;
        public object Reading
        {
            get { return this._reading; }
            set
            {
                SetProperty(this._reading, value, () => this._reading = value);
                RaisePropertyChanged(nameof(ReadingAsText));
            }
        }

        public string IconImageUri
        {
            get { return GetIconImageUri(); }
        }

        private string GetIconImageUri()
        {
            string iconImageUri;

            if (this._sensorType == SensorTypes.TEMPERATURE)
                iconImageUri = "ms-appx:///Icons/Sensor_Temperature.png";
            else if (this._sensorType == SensorTypes.STOCKFLOAT)
                iconImageUri = "ms-appx:///Icons/Sensor_StockFloat.png";
            else if (this._sensorType == SensorTypes.MOISTURE)
                iconImageUri = "ms-appx:///Icons/Sensor_Moisture.png";
            else if (this._sensorType == SensorTypes.FLOW)
                iconImageUri = "ms-appx:///Icons/Sensor_Flow.png";
            else
                iconImageUri = "ms-appx:///Icons/Sensor_Gauge.png";

            return iconImageUri;
        }


        int? _orderNumber;
        public int? OrderNumber
        {
            get { return this._orderNumber; }
            set { SetProperty(this._orderNumber, value, () => this._orderNumber = value); }
        }

        public string ReadingAsText
        {
            get
            {
                return Converters.ConvertReadingToText(Reading, this._sensorType);
            }
        }
    }
}
