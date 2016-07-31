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
            if (This.SensorType == SensorTypes.STOCKFLOAT)
            {
                var value = "ms-appx:///Icons/Sensor_StockFloat.png";
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
            set
            {
                SetProperty(this._reading, value, () => this._reading = value);
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
                return SensorModel.ReadingToText(Reading, This.SensorType);
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
                    if (sensorType == SensorTypes.PH)
                        formattedText = String.Format("{0:0.00}", reading);
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
