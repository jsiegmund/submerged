namespace Submerged.Services {

    export interface ISettingsService {
        selectedDevice: Models.DeviceModel;
        selectedSensor: Models.SensorModel;
        selectedRelay: Models.RelayModel;getSensorTypes(): any[];
        selectedModule: Models.ModuleModel;

        loadSettings(): ng.IPromise<ISettings>;

        selectDevice(device: Models.DeviceModel);
        selectSensor(sensor: Models.SensorModel);
        selectRelay(relay: Models.RelayModel);
        selectModule(module: Models.ModuleModel);

        getNewSensorName(sensorType: string): string;
        getNewModuleName(): string;
        getNewRelayName(): string;

        saveSensor(sensor: Models.SensorModel);
        saveRelay(relay: Models.RelayModel);
        saveModule(module: Models.ModuleModel);
    }

    export class SettingsService implements ISettingsService {

        public selectedDevice: Models.DeviceModel;
        public selectedSensor: Models.SensorModel;
        public selectedRelay: Models.RelayModel;
        public selectedModule: Models.ModuleModel;

        settings: ISettings;

        constructor(private dataService: Services.IDataService, private sharedService: Services.ISharedService,
            private $q: ng.IQService) {

        }

        getSensorTypes() {
            return [
                {
                    name: Statics.SENSORTYPES.TEMPERATURE,
                    displayName: "Temperature"
                }, {
                    name: Statics.SENSORTYPES.PH,
                    displayName: "pH"
                }, {
                    name: Statics.SENSORTYPES.FLOW,
                    displayName: "Flow"
                }, {
                    name: Statics.SENSORTYPES.MOISTURE,
                    displayName: "Leakage"
                }, {
                    name: Statics.SENSORTYPES.STOCKFLOAT,
                    displayName: "Stock"
                } ];
        }

        getNewSensorName(sensorType: string): string {
            var index: number = 0;
            var newName: string;
            var usedNames = <string[]>this.settings.subscription.getAllSensors().select(x => x.name);

            do {
                newName = sensorType + "_" + index;
            }
            while (usedNames.indexOf(newName) >= 0);

            return newName;
        }

        getNewModuleName(): string {
            var index: number = 0;
            var newName: string;
            var usedNames = <string[]>this.settings.subscription.getAllModules().select(x => x.name);

            do {
                newName = "module_" + index;
            }
            while (usedNames.indexOf(newName) >= 0);

            return newName;
        }

        getNewRelayName(): string {
            var index: number = 0;
            var newName: string;
            var usedNames = <string[]>this.settings.subscription.getAllRelays().select(x => x.name);

            do {
                newName = "relay_" + index;
            }
            while (usedNames.indexOf(newName) >= 0);

            return newName;
        }

        loadSettings(): ng.IPromise<ISettings> {
            var deferred = this.$q.defer<ISettings>();

            this.sharedService.loadSubscriptionFromCloud(this.dataService).then((settings) => {
                this.settings = settings;
                deferred.resolve(settings);
            });

            return deferred.promise;
        }

        selectDevice(device: Models.DeviceModel) {
            this.selectedDevice = device;
        }

        selectSensor(sensor: Models.SensorModel) {
            this.selectedSensor = sensor;
        }

        selectRelay(relay: Models.RelayModel) {
            this.selectedRelay = relay;
        }

        selectModule(module: Models.ModuleModel) {
            this.selectedModule = module;
        }

        saveSensor(sensor: Models.SensorModel): ng.IPromise<void> {
            return this.dataService.saveSensor(this.selectedDevice.deviceProperties.deviceID, sensor);
        }

        saveRelay(relay: Models.RelayModel): ng.IPromise<void> {
            return this.dataService.saveRelay(this.selectedDevice.deviceProperties.deviceID, relay);
        }

        saveModule(module: Models.ModuleModel): ng.IPromise<void> {
            return this.dataService.saveModule(this.selectedDevice.deviceProperties.deviceID, module);
        }
    }

    angular.module("ngapp").service('settingsService', SettingsService);
}