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
    public class SensorDisplayModel : NotificationBase<Sensor>
    {
        public SensorDisplayModel(Sensor sensor = null) : base(sensor)
        {
            UpdateIcon();
        }

        private void UpdateIcon()
        {
            string iconImageUrl;

            if (This.SensorType == SensorTypes.TEMPERATURE)
                iconImageUrl = "ms-appx:///Icons/Sensor_Temperature.png";
            else if (This.SensorType == SensorTypes.STOCKFLOAT)
                iconImageUrl = "ms-appx:///Icons/Sensor_StockFloat.png";
            else if (This.SensorType == SensorTypes.MOISTURE)
                iconImageUrl = "ms-appx:///Icons/Sensor_Moisture.png";
            else if (This.SensorType == SensorTypes.FLOW)
                iconImageUrl = "ms-appx:///Icons/Sensor_Flow.png";
            else
                iconImageUrl = "ms-appx:///Icons/Sensor_Gauge.png";

            SetProperty(this._iconImageUrl, iconImageUrl, () => this._iconImageUrl = iconImageUrl);
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

        public String SensorType
        {
            get { return This.SensorType; }
            set { SetProperty(This.SensorType, value, () => This.SensorType = value); UpdateIcon(); }
        }
        
        public object Reading
        {
            get { return This.Reading; }
            set
            {
                SetProperty(This.Reading, value, () => This.Reading = value);
                RaisePropertyChanged(nameof(ReadingAsText));
            }
        }

        private string _iconImageUrl;
        public string IconImageUri
        {
            get { return _iconImageUrl; }
        }

        public int? OrderNumber
        {
            get { return This.OrderNumber; }
            set { SetProperty(This.OrderNumber, value, () => This.OrderNumber = value); }
        }

        public string ReadingAsText
        {
            get
            {
                return SensorDisplayModel.ReadingToText(Reading, This.SensorType);
            }
        }

        public static string ReadingToText(object reading, string sensorType)
        {
            string formattedText = "";

            if (reading != null)
            {
                Type readingType = reading.GetType();

                if (readingType == typeof(Double) ||
                    readingType == typeof(float) ||
                    readingType == typeof(ushort))
                {
                    if (sensorType == SensorTypes.TEMPERATURE)
                        formattedText = String.Format("{0:0.0}", reading);
                    else if (sensorType == SensorTypes.PH)
                        formattedText = String.Format("{0:0.00}", reading);
                    else if (sensorType == SensorTypes.FLOW)
                        formattedText = String.Format("{0:0}", reading);
                    else
                        formattedText = String.Format("{0:0.0}", reading);
                }
                else if (reading is bool? && sensorType == SensorTypes.STOCKFLOAT)
                {
                    bool? readingAsBool = (bool?)reading;
                    if (readingAsBool.HasValue)
                        formattedText = readingAsBool.Value ? "OK" : "FILL";
                }
                else
                    formattedText = reading?.ToString();
            }

            return formattedText;
        }
    }
}
