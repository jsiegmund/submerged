namespace Submerged.Controllers {
    export class SettingsSensorController extends BaseController {

        showThresholds: boolean;
        minThreshold: number;
        maxThreshold: number;
        step: number;
        sensor: Models.SensorModel

        sensorTypes: any[];
        pinConfigOptions: string[];
        moduleOptions: Models.ModuleModel[];


        constructor(private subscriptionService: Services.ISubscriptionService, private $scope: ng.IRootScopeService,
            private $location: ng.ILocationService, $mdToast: ng.material.IToastService, menuService: Services.IMenuService) {

            super($mdToast);

            this.sensorTypes = subscriptionService.getSensorTypes();
            this.pinConfigOptions = subscriptionService.getPinList();
            this.moduleOptions = subscriptionService.getSelectedDevice().modules;
            this.sensor = subscriptionService.getSelectedSensor();
            this.setThresholdVariables();

            $scope.$watch((scope) => {
                return this.sensor.sensorType;
            }, (newValue, oldValue) => {
                this.setThresholdVariables();
                });

            // add delete button to remove existing sensor
            if (this.sensor.name != undefined) {
                var deleteButton = new Services.CommandButton();
                deleteButton.svgSrc = 'icons/ic_delete_24px.svg';
                deleteButton.clickHandler = this.delete;
                deleteButton.label = 'Delete';
                deleteButton.owner = this;

                menuService.setButtons([deleteButton]);
            }
        }

        delete() {
            this.busy = true;

            this.subscriptionService.deleteSensor(this.sensor).then(() => {
                window.history.back();      // sensor deleted; go back
                this.busy = false;
            }, () => {
                this.showSimpleToast("Sorry, settings were not saved.");
                this.busy = false;
            });
        }

        save() {
            this.busy = true;

            this.subscriptionService.saveSensor(this.sensor).then(() => {
                this.showSimpleToast("Sensor settings saved!");
            }, (error) => {
                console.log(`Failure saving sensor settings: ${error}`);
                this.showSimpleToast("Sorry, settings were not saved.");
            }).finally(() => {
                this.busy = false;
            });
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
