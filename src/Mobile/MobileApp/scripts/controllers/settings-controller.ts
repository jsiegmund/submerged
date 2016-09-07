namespace Submerged.Controllers {

    class DeviceDisplayModel extends Models.DeviceModel {
        selected: boolean;
    }

    class ModuleDisplayModel extends Models.ModuleModel {
        sensorCount: number;
    }

    export class SettingsController {
        loading: boolean = true;
        saving: boolean = false;
        newDeviceEnabled: boolean = true;

        tanks: Models.TankModel[];
        devices: DeviceDisplayModel[];
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
            private $q: ng.IQService, private subscriptionService: Services.ISubscriptionService, private menuService: Services.IMenuService) {

            this.deviceId = subscriptionService.getSelectedDeviceID();

            this.selectedDevice = subscriptionService.getSelectedDevice();
            this.selectedSensor = subscriptionService.getSelectedSensor();
            this.selectedRelay = subscriptionService.getSelectedRelay();
            this.selectedModule = subscriptionService.getSelectedModule();
            this.sensorTypes = subscriptionService.getSensorTypes();

            // at the moment we only allow the maximum number of 1 device
            this.newDeviceEnabled = subscriptionService.getDeviceCount() < 1;

            this.loadData();
        }

        newTank() {
            this.subscriptionService.selectTank(new Models.TankModel());
            this.$location.path("/settings/tank");
        }

        openTank(tank: Models.TankModel) {
            this.subscriptionService.selectTank(tank);
            this.$location.path("/settings/tank");
        }

        newDevice() {
            this.subscriptionService.selectDevice(null);
            this.$location.path("/settings/device-edit");
        }

        openDevice(device: Models.DeviceModel) {
            this.subscriptionService.selectDevice(device);
            this.$location.path("/settings/device");
        }

        newModule() {
            this.subscriptionService.selectModule(null);
            this.$location.path("/settings/module");
        }

        openModule(module: Models.ModuleModel) {
            this.subscriptionService.selectModule(module);
            this.$location.path("/settings/module");
        }

        newRelay() {
            this.subscriptionService.selectRelay(null);
            this.$location.path("/settings/relay");
        }

        openRelay(relay: Models.RelayModel) {
            this.subscriptionService.selectRelay(relay);
            this.$location.path("/settings/relay");
        }

        newSensor() {
            this.subscriptionService.selectSensor(null);
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
                this.devices = <DeviceDisplayModel[]>subscription.devices;

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

        deleteDevice() {
            var selected = this.devices.where({ selected: true });
            var promises = [];

            if (selected.length != 1) {
                this.showSimpleToast("Please select 1 device to delete.");
                return;
            }

            window.navigator.notification.confirm("Are you sure you want to delete this device?", () => {
                this.subscriptionService.deleteDevice(selected[0]).then(() => {
                    this.showSimpleToast("The device has been deleted.");
                }, () => {
                    this.showSimpleToast("Something went wrong.");
                });
            }, "Delete device?");
        }

        deviceSelectionChanged(device: DeviceDisplayModel) {
            // allow only one selection, so toggle all others back to false
            var others = this.devices.where(x => x.selected == true && x.deviceProperties.deviceID != device.deviceProperties.deviceID);
            for (var other of others) {
                other.selected = false;
            }

            // determine whether we should show the delete button
            var selected = this.devices.any({ selected: true });

            if (selected && this.menuService.getButtons().length == 0) {
                var deleteButton = new Services.CommandButton();
                deleteButton.svgSrc = 'icons/ic_delete_24px.svg';
                deleteButton.clickHandler = this.deleteDevice;
                deleteButton.label = 'Delete';
                deleteButton.owner = this;

                this.menuService.setButtons([deleteButton]);
            }
            else if (!selected && this.menuService.getButtons().length > 0) {
                this.menuService.setButtons([]);
            }
        }

        toggleDeviceSelected(device: DeviceDisplayModel) {
            device.selected = !device.selected;
            this.deviceSelectionChanged(device);
        };

        deviceClicked(device: DeviceDisplayModel, $event: ng.IAngularEvent): void {
            var selected = this.devices.any({ selected: true });

            if (!selected) {
                this.openDevice(device);
            }
            else
                this.toggleDeviceSelected(device);
        }
    }

    angular.module("ngapp").controller("SettingsController", SettingsController);
}