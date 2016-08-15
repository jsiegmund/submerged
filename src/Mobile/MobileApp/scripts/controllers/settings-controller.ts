namespace Submerged.Controllers {

    class ModuleDisplayModel extends Models.ModuleModel {
        sensorCount: number;
    }

    export class SettingsController {
        loading: boolean = true;
        saving: boolean = false;

        tanks: Models.TankModel[];
        devices: Models.DeviceModel[];
        sensorTypes: any[];

        selectedTabIndex: number;

        selectedDevice: Models.DeviceModel;
        selectedSensor: Models.SensorModel;
        selectedRelay: Models.RelayModel;
        selectedModule: Models.ModuleModel;

        indexedRules = [];

        deviceId: string;

        constructor(private mobileService: Services.IMobileService, private fileService: Services.IFileService,
            private $scope: ng.IRootScopeService, private $location: ng.ILocationService, private $mdToast: ng.material.IToastService,
            private $q: ng.IQService, private subscriptionService: Services.ISubscriptionService) {

            this.deviceId = subscriptionService.getSelectedDeviceId();

            this.selectedDevice = subscriptionService.getSelectedDevice();
            this.selectedSensor = subscriptionService.getSelectedSensor();
            this.selectedRelay = subscriptionService.getSelectedRelay();
            this.selectedModule = subscriptionService.getSelectedModule();
            this.sensorTypes = subscriptionService.getSensorTypes();

            this.loadData();
        }

        openTank(tank: Models.TankModel) {
            this.subscriptionService.selectTank(tank);
            this.$location.path("/settings/tank");
        }

        newTank() {
            this.subscriptionService.selectTank(new Models.TankModel());
            this.$location.path("/settings/tank");
        }

        openDevice(device: Models.DeviceModel) {
            this.subscriptionService.selectDevice(device);
            this.$location.path("/settings/device");
        }

        newModule() {
            this.subscriptionService.selectModule(new Models.ModuleModel());
            this.$location.path("/settings/module");
        }

        openModule(module: Models.ModuleModel) {
            this.subscriptionService.selectModule(module);
            this.$location.path("/settings/module");
        }

        newRelay() {
            this.subscriptionService.selectRelay(new Models.RelayModel());
            this.$location.path("/settings/relay");
        }

        openRelay(relay: Models.RelayModel) {
            this.subscriptionService.selectRelay(relay);
            this.$location.path("/settings/relay");
        }

        newSensor() {
            this.subscriptionService.selectSensor(new Models.SensorModel());
            this.$location.path("/settings/sensor");
        }

        openSensor(sensor: Models.SensorModel) {
            this.subscriptionService.selectSensor(sensor);
            this.$location.path("/settings/sensor");
        }

        showSensorSection(type: string): boolean {
            var sensors = this.subscriptionService.getSelectedDevice().sensors.where({ sensorType: type });
            return sensors.length > 0;
        }

        sensorBySensorType = function (type: string) {
            return function (sensor: Models.SensorModel) {
                return sensor.sensorType === type;
            }
        }

        loadData(): void {
            this.loading = true;

            this.subscriptionService.load().then((subscription) => {

                this.tanks = subscription.tanks;
                this.devices = subscription.devices;

                this.loading = false;
            },
                () => { this.loading = false; }
            );
        }
        
        showSimpleToast(text: string): void {
            this.$mdToast.show(
                this.$mdToast.simple()
                    .textContent(text)
                    .position("top right")
                    .hideDelay(3000)
            );
        }
    }

    angular.module("ngapp").controller("SettingsController", SettingsController);
}