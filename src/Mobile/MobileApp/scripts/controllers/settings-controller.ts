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

        constructor(private sharedService: Services.ISharedService, private mobileService: Services.IMobileService, private fileService: Services.IFileService,
            private $scope: ng.IRootScopeService, private $location: ng.ILocationService, private $mdToast: ng.material.IToastService,
            private $q: ng.IQService, private settingsService: Services.ISettingsService) {

            this.deviceId = sharedService.settings.getDeviceId();

            this.selectedDevice = settingsService.selectedDevice;
            this.selectedSensor = settingsService.selectedSensor;
            this.selectedRelay = settingsService.selectedRelay;
            this.selectedModule = settingsService.selectedModule;
            this.sensorTypes = settingsService.getSensorTypes();

            this.loadData();
        }

        openDevice(device: Models.DeviceModel) {
            this.settingsService.selectDevice(device);
            this.$location.path("/settings/device");
        }

        newModule() {
            this.settingsService.selectModule(new Models.ModuleModel());
            this.$location.path("/settings/module");
        }

        openModule(module: Models.ModuleModel) {
            this.settingsService.selectModule(module);
            this.$location.path("/settings/module");
        }

        newRelay() {
            this.settingsService.selectRelay(new Models.RelayModel());
            this.$location.path("/settings/relay");
        }

        openRelay(relay: Models.RelayModel) {
            this.settingsService.selectRelay(relay);
            this.$location.path("/settings/relay");
        }

        newSensor() {
            this.settingsService.selectSensor(new Models.SensorModel());
            this.$location.path("/settings/sensor");
        }

        openSensor(sensor: Models.SensorModel) {
            this.settingsService.selectSensor(sensor);
            this.$location.path("/settings/sensor");
        }

        sensorBySensorType = function (type: string) {
            return function (sensor: Models.SensorModel) {
                return sensor.sensorType === type;
            }
        }

        loadData(): void {
            this.loading = true;

            this.settingsService.loadSettings().then((settings) => {

                this.tanks = settings.subscription.tanks;
                this.devices = settings.subscription.devices;

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