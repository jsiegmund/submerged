namespace Submerged.Services {

    export interface ISubscriptionService {
        getSensorTypes(): any[];
        getModuleTypes(): any[];
        getPinList(): string[];
        getSelectedDeviceID(): string;

        getSelectedDevice(): Models.DeviceModel;
        getSelectedSensor(): Models.SensorModel;
        getSelectedRelay(): Models.RelayModel;
        getSelectedModule(): Models.ModuleModel;
        getSelectedTank(): Models.TankModel;

        selectDevice(device: Models.DeviceModel);
        selectSensor(sensor: Models.SensorModel);
        selectRelay(relay: Models.RelayModel);
        selectModule(module: Models.ModuleModel);
        selectTank(tank: Models.TankModel);

        getNewSensorName(sensorType: string): string;
        getNewModuleName(): string;
        getNewRelayName(): string;

        saveSensor(sensor: Models.SensorModel): ng.IPromise<void>;
        saveRelay(relay: Models.RelayModel): ng.IPromise<void>;
        saveModule(module: Models.ModuleModel): ng.IPromise<void>;
        saveTank(tank: Models.TankModel): ng.IPromise<void>;
        saveDevice(device: Models.DeviceModel): ng.IPromise<void>;

        deleteSensor(sensor: Models.SensorModel): ng.IPromise<void>;
        deleteRelay(relay: Models.RelayModel): ng.IPromise<void>;
        deleteModule(module: Models.ModuleModel): ng.IPromise<void>;
        deleteTank(tank: Models.TankModel): ng.IPromise<void>;
        deleteDevice(device: Models.DeviceModel): ng.IPromise<void>;

        load(forceRefresh?: boolean): ng.IPromise<Models.SubscriptionModel>;
        getDeviceCount(): number;
    }

    export class SubscriptionService implements ISubscriptionService {

        private selectedDevice: string;
        private selectedSensor: string;
        private selectedRelay: string;
        private selectedModule: string;
        private selectedTank: string;

        subscription: Models.SubscriptionModel;
        file: string = "subscription.json";

        constructor(private dataService: Services.IDataService,
            private $q: ng.IQService, private fileService: Services.IFileService) {

        }

        getDeviceCount(): number {
            return this.subscription.devices.length;
        }

        getSelectedDeviceID(): string {
            if (this.selectedDevice == undefined) {
                if (this.subscription != null && this.subscription.devices.length > 0)
                    this.selectedDevice = this.subscription.devices.first().deviceProperties.deviceID;
                else
                    console.log("Called getSelectedDeviceId before subscription was loaded, or there is no device.");
            }

            return this.selectedDevice;
        }

        load(forceRefresh: boolean = false): ng.IPromise<Models.SubscriptionModel> {
            var deferred = this.$q.defer<Models.SubscriptionModel>();

            if (this.subscription == undefined || forceRefresh) {

                var errorCallback = () => { deferred.reject(); };

                // when not forced to refresh; first try a local file restore
                // otherwise; always load from the cloud first
                if (!forceRefresh) {
                    this.loadFromFile().then((subscription) => {
                        deferred.resolve(this.subscription);
                    }, () => {
                        console.log("Failure loading subscription from file, reverting to cloud");
                        this.loadFromCloud().then((subscription) => {
                            deferred.resolve(subscription);
                        }, errorCallback);
                    });
                } else {
                    this.loadFromCloud().then((subscription) => {
                        deferred.resolve(subscription);
                    }, errorCallback);
                }
            } else {
                console.log("Subscription loaded from local instance, no refresh");
                deferred.resolve(this.subscription);
            }

            return deferred.promise;
        }

        private loadFromFile(): ng.IPromise<Models.SubscriptionModel> {
            var deferred = this.$q.defer<Models.SubscriptionModel>();
            var folder = window.cordova.file.applicationStorageDirectory;

            this.fileService.getJsonFile<Models.SubscriptionModel>(this.file, folder).then(
                (subscription) => {
                    if (subscription) {
                        console.log("Subscription loaded from file");
                        this.subscription = subscription;
                        deferred.resolve(subscription);
                    } else {
                        console.log("Subscription not loaded from file (null)");
                        deferred.reject();
                    }
                }, (err) => {
                    console.log("Subscription not loaded from file. Error: " + err);
                    deferred.reject();
                }
            );

            return deferred.promise;
        }

        private loadFromCloud(): ng.IPromise<Models.SubscriptionModel> {
            var deferred = this.$q.defer<Models.SubscriptionModel>();

            this.dataService.getSubscription().then(
                (subscription) => {
                    if (subscription) {
                        console.log("Subscription loaded from the cloud");
                        this.subscription = subscription;
                        deferred.resolve(subscription);
                    } else {
                        console.log("Subscription not loaded from the cloud (null)");
                        deferred.reject();
                    }
                }, (err) => {
                    console.log("Subscription not loaded from the cloud. Error: " + err);
                    deferred.reject();
                });

            return deferred.promise;
        }

        private saveSubscriptionFile(): ng.IPromise<void> {
            var folder = window.cordova.file.applicationStorageDirectory;
            return this.fileService.storeJsonFile(this.file, folder, this.subscription);
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

        getModuleTypes() {
            return [
                {
                    name: Statics.MODULETYPES.FIRMATA,
                    displayName: "Generic Firmata"
                }, {
                    name: Statics.MODULETYPES.SENSORS,
                    displayName: "Predefined Sensors module"
                }, {
                    name: Statics.MODULETYPES.CABINET,
                    displayName: "Predefined Cabinet module"
                }];
        }

        getAllSensors(): Models.SensorModel[] {
            var result: Models.SensorModel[] = new Array<Models.SensorModel>();

            for (var device of this.subscription.devices) {
                result = result.concat(device.sensors);
            }

            return result;
        }

        getAllModules(): Models.ModuleModel[] {
            var result: Models.ModuleModel[] = new Array<Models.ModuleModel>();

            for (var device of this.subscription.devices) {
                result = result.concat(device.modules);
            }

            return result;
        }

        getAllRelays(): Models.RelayModel[] {
            var result: Models.RelayModel[] = new Array<Models.RelayModel>();

            for (var device of this.subscription.devices) {
                result = result.concat(device.relays);
            }

            return result;
        }

        getPinList(): string[] {
            return ["A0", "A1", "A2", "A3", "A4", "A5", "A6", "A7", "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D10", "D11", "D12", "D13"];
        }

        getNewSensorName(sensorType: string): string {
            var index: number = 0;
            var newName: string;
            var usedNames = <string[]>this.getAllSensors().select(x => x.name);

            do {
                newName = sensorType + "_" + index++;
            }
            while (usedNames.indexOf(newName) >= 0);

            return newName;
        }

        getNewModuleName(): string {
            var index: number = 0;
            var newName: string;
            var usedNames = <string[]>this.getAllModules().select(x => x.name);

            do {
                newName = "module_" + index++;
            }
            while (usedNames.indexOf(newName) >= 0);

            return newName;
        }

        getNewRelayName(): string {
            var index: number = 0;
            var newName: string;
            var usedNames = <string[]>this.getAllRelays().select(x => x.name);

            do {
                newName = "relay_" + index++;
            }
            while (usedNames.indexOf(newName) >= 0);

            return newName;
        }

        getNewTankName(): string {
            var index: number = 0;
            var newName: string;
            var usedNames = <string[]>this.subscription.tanks.select(x => x.name);

            do {
                newName = "tank_" + index++;
            }
            while (usedNames.indexOf(newName) >= 0);

            return newName;
        }

        getSelectedDevice(): Models.DeviceModel {
            var result = this.subscription.devices.firstOrDefault(x => x.deviceProperties.deviceID == this.selectedDevice);

            if (!result)
                result = new Models.DeviceModel();

            return result;
        }

        selectDevice(device: Models.DeviceModel) {
            if (device && device.deviceProperties && device.deviceProperties.deviceID)
                this.selectedDevice = device.deviceProperties.deviceID;
            else
                this.selectedDevice = null;
        }

        getSelectedTank(): Models.TankModel {
            var tank: Models.TankModel;

            tank = this.subscription.tanks.firstOrDefault({ name: this.selectedTank });

            if (!tank)
                tank = new Models.TankModel();

            return tank;
        }

        selectTank(tank: Models.TankModel) {
            this.selectedTank = tank.name;
        }

        getSelectedSensor(): Models.SensorModel {
            var device = this.getSelectedDevice();
            var sensor: Models.SensorModel;

            if (device && device.sensors)
                sensor = device.sensors.firstOrDefault({ name: this.selectedSensor });

            if (!sensor)
                sensor = new Models.SensorModel();

            return sensor;
        }

        selectSensor(sensor: Models.SensorModel) {
            this.selectedSensor = sensor.name;
        }

        getSelectedRelay(): Models.RelayModel {
            var device = this.getSelectedDevice();
            var relay: Models.RelayModel;

            if (device && device.relays)
                relay = device.relays.firstOrDefault({ name: this.selectedRelay });

            if (!relay)
                relay = new Models.RelayModel;

            return relay;
        }

        selectRelay(relay: Models.RelayModel) {
            this.selectedRelay = relay.name;
        }

        getSelectedModule(): Models.ModuleModel {
            var device = this.getSelectedDevice();
            var module: Models.ModuleModel;

            if (device && device.modules)
                module = device.modules.firstOrDefault({ name: this.selectedModule });

            if (!module)
                module = new Models.ModuleModel();

            return module;
        }

        selectModule(module: Models.ModuleModel) {
            this.selectedModule = module.name;
        }

        saveDevice(device: Models.DeviceModel): ng.IPromise<void> {
            if (device.deviceProperties.primaryKey == undefined) {
                return this.dataService.addDevice(device).then((newDevice) => {
                    this.subscription.devices.push(newDevice);
                    this.saveSubscriptionFile();
                });
            }
            else {
                return this.dataService.updateDevice(device).then(() => {
                    var oldDevice = this.subscription.devices.firstOrDefault(x => x.deviceProperties.deviceID == device.deviceProperties.deviceID);
                    var index = this.subscription.devices.indexOf(oldDevice);
                    this.subscription.devices.splice(index, 1, device);

                    this.saveSubscriptionFile();
                });
            }
        }

        deleteDevice(device: Models.DeviceModel): ng.IPromise<void> {
            return this.dataService.deleteDevice(device).then(() => {
                var oldDevice = this.subscription.devices.firstOrDefault(x => x.deviceProperties.deviceID == device.deviceProperties.deviceID);
                var index = this.subscription.devices.indexOf(oldDevice);
                this.subscription.devices.splice(index, 1);

                this.saveSubscriptionFile();
            });
        }

        saveSensor(sensor: Models.SensorModel): ng.IPromise<void> {
            //var method: (deviceID: string, sensor: Models.SensorModel) => ng.IPromise<void>;

            if (sensor.name == undefined) {
                sensor.name = this.getNewSensorName(sensor.sensorType);

                return this.dataService.addSensor(this.selectedDevice, sensor).then(() => {
                    // add the newly added sensor to the subscription
                    this.getSelectedDevice().sensors.push(sensor);

                    this.saveSubscriptionFile();
                });
            }
            else {
                return this.dataService.updateSensor(this.selectedDevice, sensor).then(() => {
                    // update the sensor in the subscription
                    var device = this.getSelectedDevice();
                    var oldSensor = device.sensors.firstOrDefault({ name: sensor.name });
                    var index = device.sensors.indexOf(oldSensor);
                    device.sensors.splice(index, 1, sensor);

                    this.saveSubscriptionFile();
                });
            }
        }

        saveRelay(relay: Models.RelayModel): ng.IPromise<void> {
            if (!relay.name) {
                relay.name = this.getNewRelayName();

                return this.dataService.addRelay(this.selectedDevice, relay).then(() => {
                    this.getSelectedDevice().relays.push(relay);
                    this.saveSubscriptionFile();
                });
            }
            else {
                return this.dataService.updateRelay(this.selectedDevice, relay).then(() => {
                    var device = this.getSelectedDevice();
                    var oldRelay = device.relays.firstOrDefault({ name: relay.name });
                    var index = device.relays.indexOf(oldRelay);
                    device.relays.splice(index, 1, relay);

                    this.saveSubscriptionFile();
                });
            }
        }

        saveModule(module: Models.ModuleModel): ng.IPromise<void> {
            if (!module.name) {
                module.name = this.getNewModuleName();

                return this.dataService.addModule(this.selectedDevice, module).then(() => {
                    this.getSelectedDevice().modules.push(module);
                    this.saveSubscriptionFile();
                });
            }
            else {
                return this.dataService.updateModule(this.selectedDevice, module).then(() => {
                    var device = this.getSelectedDevice();
                    var oldModule = device.modules.firstOrDefault({ name: module.name });
                    var index = device.modules.indexOf(oldModule);
                    device.modules.splice(index, 1, module);

                    this.saveSubscriptionFile();
                });
            }
        }

        saveTank(tank: Models.TankModel): ng.IPromise<void> {
            if (!tank.name) {
                tank.name = this.getNewTankName();

                return this.dataService.addTank(tank).then(() => {
                    this.subscription.tanks.push(tank);
                });
            }
            else {
                return this.dataService.updateTank(tank).then(() => {
                    var oldTank = this.subscription.tanks.firstOrDefault({ name: tank.name });
                    var index = this.subscription.tanks.indexOf(oldTank);
                    this.subscription.tanks.splice(index, 1, tank);

                    this.saveSubscriptionFile();
                });
            }
        }

        deleteTank(tank: Models.TankModel): ng.IPromise<void> {
            var tankName = tank.name;
            return this.dataService.deleteTank(tank).then(() => {
                var tank = this.subscription.tanks.firstOrDefault({ name: tankName });
                var index = this.subscription.tanks.indexOf(tank);
                this.subscription.tanks.splice(index, 1);

                this.saveSubscriptionFile();
            });
        }

        deleteSensor(sensor: Models.SensorModel): ng.IPromise<void> {
            return this.dataService.deleteSensor(this.selectedDevice, sensor).then(() => {
                // update the sensor in the subscription
                var device = this.getSelectedDevice();
                var oldSensor = device.sensors.firstOrDefault({ name: sensor.name });
                var index = device.sensors.indexOf(oldSensor);
                device.sensors.splice(index, 1);

                this.saveSubscriptionFile();
            });
        }

        deleteRelay(relay: Models.RelayModel): ng.IPromise<void> {
            return this.dataService.deleteRelay(this.selectedDevice, relay).then(() => {
                // remove the relay from the subscription when call succeeded
                var device = this.getSelectedDevice();
                var oldRelay = device.relays.firstOrDefault({ name: relay.name });
                var index = device.relays.indexOf(oldRelay);
                device.relays.splice(index, 1);

                this.saveSubscriptionFile();
            });
        }

        deleteModule(module: Models.ModuleModel): ng.IPromise<void> {
            return this.dataService.deleteModule(this.selectedDevice, module).then(() => {
                var device = this.getSelectedDevice();
                var oldModule = device.modules.firstOrDefault({ name: module.name });
                var index = device.modules.lastIndexOf(oldModule);
                device.modules.splice(index, 1);

                this.saveSubscriptionFile();
            });
        }
    }

    angular.module("ngapp").service('subscriptionService', SubscriptionService);
}