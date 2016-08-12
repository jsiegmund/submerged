namespace Submerged.Controllers {
    export class SettingsSensorController {

        showThresholds: boolean;
        minThreshold: number;
        maxThreshold: number;
        step: number;
        sensor: Models.SensorModel
        sensorTypes: any[];

        constructor(private settingsService: Services.ISettingsService, private $scope: ng.IRootScopeService,
            private $location: ng.ILocationService) {
            this.sensorTypes = settingsService.getSensorTypes();
            this.sensor = settingsService.selectedSensor;
            this.setThresholdVariables();

            $scope.$watch((scope) => {
                return this.sensor.sensorType;
            }, (newValue, oldValue) => {
                this.setThresholdVariables();
            });
        }

        save() {
            if (this.sensor.name == undefined) {
                this.sensor.name = this.settingsService.getNewSensorName(this.sensor.sensorType);
            }

            this.settingsService.saveSensor(this.sensor);
        }

        setThresholdVariables(): void {
            switch (this.sensor.sensorType) {
                case Statics.SENSORTYPES.TEMPERATURE:
                    this.minThreshold = 10;
                    this.maxThreshold = 40;
                    this.step = 1;
                    this.showThresholds = true;
                    break;
                case Statics.SENSORTYPES.PH:
                    this.minThreshold = 5.5;
                    this.maxThreshold = 8;
                    this.step = 0.1;
                    this.showThresholds = true;
                    break;
                case Statics.SENSORTYPES.FLOW:
                    this.minThreshold = 1000;
                    this.maxThreshold = 10000;
                    this.step = 100;
                    this.showThresholds = true;
                    break;
                default:
                    this.minThreshold = 1;
                    this.maxThreshold = 1;
                    this.step = 1;
                    this.showThresholds = false;
            }
        }

    }

    angular.module("ngapp").controller("SettingsSensorController", SettingsSensorController);
}
