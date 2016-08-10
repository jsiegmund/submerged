namespace Submerged.Directives {

    export interface ISensorController {
        formatSensorValue(sensor: Models.SensorModel, value: any): any;
        calculateSensorClass(sensor: Models.SensorModel): string;
    }

    export class SensorController implements ISensorController {
        static $inject = ['$sce'];
        constructor(private $sce: ng.ISCEService) {
            // $location and toaster are now properties of the controller
        }

        formatSensorValue = (sensor: Models.SensorModel, value: any): any => {
            var result: string = "";
            if (value != null) {
                switch (sensor.sensorType) {
                    case "temperature":
                        result = value.toFixed(1) + '&deg;';
                        break;
                    case "pH":
                        result = value.toFixed(2);
                        break;
                    default:
                        result = value.toString();
                        break;
                }
            }

            return this.$sce.trustAsHtml(result);
        }

        calculateSensorClass = (sensor: Models.SensorModel): string => {
            switch (sensor.sensorType) {
                case Statics.SENSORTYPES.FLOW:
                    return this.calculateSensorClassByThreshold(sensor);
                case Statics.SENSORTYPES.MOISTURE:
                    return this.calculateSensorClassByBool(sensor, false, true);
                case Statics.SENSORTYPES.PH:
                    return this.calculateSensorClassByThreshold(sensor);
                case Statics.SENSORTYPES.STOCKFLOAT:
                    return this.calculateSensorClassByBool(sensor, false, true);
                case Statics.SENSORTYPES.TEMPERATURE:
                    return this.calculateSensorClassByThreshold(sensor);
            }
        }

        calculateSensorClassByBool = (sensor: Models.SensorModel, okState: boolean, nullIsOk: boolean): string => {
            var result: boolean;

            if (sensor.reading === undefined || sensor.reading === null)
                result = nullIsOk;
            else
                result = sensor.reading === okState;

            if (result)
                return "md-fab npt-kpigreen";
            else
                return "md-fab npt-kpired";
        }

        calculateSensorClassByThreshold = (sensor: Models.SensorModel): string => {
            if (sensor === null) {
                return "md-fab npt-kpigray";
            }

            // find the low and high rules for this sensor
            var lowValue = sensor.minThreshold;
            var highValue = sensor.maxThreshold;

            var deviation = (highValue - lowValue) * 0.1;
            var orangeLowValue = lowValue + deviation;
            var orangeHighValue = highValue - deviation;

            if (sensor.reading < orangeHighValue && sensor.reading > orangeLowValue)
                return "md-fab npt-kpigreen";
            else if (sensor.reading < lowValue || sensor.reading > highValue)
                return "md-fab npt-kpired";
            else
                return "md-fab npt-kpiorange";
        }
    }
}