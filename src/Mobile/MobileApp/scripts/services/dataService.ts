namespace Submerged.Services {
    export interface IDataService {
        toggleMaintenance(deviceId: string, maintenanceMode: boolean): ng.IPromise<void>;

        getSensors(deviceId: string): ng.IPromise<Models.SensorModel[]>;
        saveSensors(deviceId: string, sensors: Models.SensorModel[]): ng.IPromise<void>;
        saveSensor(deviceId: string, sensor: Models.SensorModel): ng.IPromise<void>;

        getRelays(deviceId: string): ng.IPromise<Models.RelayModel[]>;
        saveRelay(deviceId: string, relay: Models.RelayModel): ng.IPromise<void>;
        toggleRelay(deviceId: string, relayNumber: number, relayState: boolean): ng.IPromise<void>;

        getTankLogs(tankId: string): ng.IPromise<Models.TankLogModel[]>;
        saveTankLog(newLog: Models.TankLogModel): ng.IPromise<void>;

        getModules(deviceId: string): ng.IPromise<Models.ModuleModel[]>;
        saveModule(deviceId: string, module: Models.ModuleModel);

        getTelemetry(deviceId: string): ng.IPromise<any>;
        getTelemetryLastThreeHours(deviceId: string, date: Date): ng.IPromise<any>;

        getData(rangeType: string, date: Date, deviceId: string): ng.IPromise<Models.AnalyticsDataModel>;

        getSubscription(): ng.IPromise<Models.SubscriptionModel>;  
    }

    export class DataService implements IDataService {
        
        timezoneOffsetSeconds: number;

        constructor(private mobileService: IMobileService, private $q: ng.IQService, private sharedService: Services.ISharedService) {
            this.timezoneOffsetSeconds = sharedService.settings.globalizationInfo.server_offset_seconds;
        }

        getSubscription(): ng.IPromise<Models.SubscriptionModel> {
            var deferred = this.$q.defer<Models.SubscriptionModel>();
            var apiUrl = "subscription";

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, (error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /subscription to load the subscription details: " + error);
                    deferred.reject();
                }
                else {
                    deferred.resolve(success.result);
                }
            });

            return deferred.promise;
        }


        getData(rangeType: string, date: Date, deviceId: string): ng.IPromise<Models.AnalyticsDataModel> {
            var apiUrl = "data/" + rangeType + "?deviceId=" + deviceId + "&date=" + date.toISOString() + "&offset=" + this.timezoneOffsetSeconds;
            var deferred = this.$q.defer<Models.AnalyticsDataModel>();

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, (error, success) => {
                if (error) {
                    console.log("Failure getting data from API: " + error);
                    deferred.reject();
                }
                else {
                    var result: Models.AnalyticsDataModel = success.result;
                    deferred.resolve(result);
                }
            });

            return deferred.promise;
        }

        getTelemetryLastThreeHours(deviceId: string, date: Date): ng.IPromise<any> {
            var apiUrl = "data/threehours?deviceId=" + deviceId + "&date=" + date.toISOString() + "&offset=" + this.timezoneOffsetSeconds;
            var deferred = this.$q.defer<any>();

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, (error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /data/threehours: " + error);
                    deferred.reject();
                }
                else {
                    deferred.resolve(success.result);
                }
            });

            return deferred.promise;
        }

        getTelemetry(deviceId: string): ng.IPromise<Models.TelemetryModel> {
            var apiUrl = "data/latest?deviceId=" + deviceId;
            var deferred = this.$q.defer<Models.TelemetryModel>();

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, (error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /data/latest: " + error);
                    deferred.reject();
                }
                else {
                    var result: Models.TelemetryModel = success.result;
                    deferred.resolve(result);
                }
            });

            return deferred.promise;
        }       

        getModules(deviceId: string): ng.IPromise<Models.ModuleModel[]> {
            var apiUrl = "modules?deviceId=" + deviceId;
            var deferred = this.$q.defer<Models.ModuleModel[]>();

            // get the latest available data record to show untill it's updated
            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, (error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /modules: " + error);
                    deferred.reject();
                }
                else {
                    var result: Models.ModuleModel[] = success.result;
                    deferred.resolve(result);      // process the last known data for display
                }
            });

            return deferred.promise;
        }

        saveModule(deviceId: string, module: Models.ModuleModel): ng.IPromise<void> {
            var apiUrl = "modules?deviceId=" + deviceId;
            var deferred = this.$q.defer<void>();

            this.mobileService.invokeApi(apiUrl, {
                body: module,
                method: "put"
            }, (error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /sensors to save a module: " + error);
                    deferred.reject();
                }
                else {
                    deferred.resolve();
                }
            });

            return deferred.promise;
        }

        saveSensors(deviceId: string, sensors: Models.SensorModel[]): ng.IPromise<void> {
            var apiUrl = "sensors/save?deviceId=" + deviceId;
            var deferred = this.$q.defer<void>();

            this.mobileService.invokeApi(apiUrl, {
                body: sensors,
                method: "post"
            }, (error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /sensors/save to save the sensor configuration: " + error);
                    deferred.reject();
                }
                else {
                    deferred.resolve();
                }
            });

            return deferred.promise;
        }

        saveSensor(deviceId: string, sensor: Models.SensorModel): ng.IPromise<void> {
            var apiUrl = "sensors?deviceId=" + deviceId;
            var deferred = this.$q.defer<void>();

            this.mobileService.invokeApi(apiUrl, {
                body: sensor,
                method: "put"
            }, (error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /sensors to save a sensor: " + error);
                    deferred.reject();
                }
                else {
                    deferred.resolve();
                }
            });

            return deferred.promise;
        }

        getSensors(deviceId: string): ng.IPromise<Models.SensorModel[]> {
            var apiUrl = "sensors?deviceId=" + deviceId;
            var deferred = this.$q.defer<Models.SensorModel[]>();

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, (error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /data/sensors to get the sensor configuration: " + error);
                    deferred.reject();
                }
                else {
                    var result: Models.SensorModel[] = success.result;
                    deferred.resolve(result);
                }
            });

            return deferred.promise;
        }

        toggleMaintenance(deviceId: string, maintenanceMode: boolean): ng.IPromise<void> {
            var apiUrl = "control/maintenance/toggle?deviceId=" + deviceId + "&inMaintenance=" + maintenanceMode;
            var deferred = this.$q.defer<void>();

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, (error, success) => {
                if (error) {
                    console.log("Error calling /control/maintenance/toggle to toggle maintenance state: " + error);
                    deferred.reject();
                }
                else {
                    deferred.resolve();
                }
            });


            return deferred.promise;
        }

        toggleRelay(deviceId: string, relayNumber: number, relayState: boolean): ng.IPromise<void> {
            var apiUrl = "control/setrelay?deviceId=" + deviceId + "&relayNumber=" + relayNumber + "&state=" + relayState;
            var deferred = this.$q.defer<void>();

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, (error, success) => {
                if (error) {
                    console.log("Error calling /control/setrelay to toggle relay state: " + error);
                    deferred.reject();
                }
                else {
                    deferred.resolve();
                }
            });

            return deferred.promise;
        }

        saveRelay(deviceId: string, relay: Models.RelayModel): ng.IPromise<void> {
            var apiUrl = "relays?deviceId=" + deviceId;
            var deferred = this.$q.defer<void>();

            this.mobileService.invokeApi(apiUrl, {
                body: relay,
                method: "post"
            }, (error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /relays to save the sensor configuration: " + error);
                    deferred.reject();
                }
                else {
                    deferred.resolve();
                }
            });

            return deferred.promise;
        }

        getRelays(deviceId: string): ng.IPromise<Models.RelayModel[]> {
            var apiUrl = "control/relays?deviceId=" + deviceId;
            var deferred = this.$q.defer<Models.RelayModel[]>();

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, (error, success) => {
                if (error) {
                    console.log("Error calling /control/relays to get relay configuration: " + error);
                    deferred.reject();
                }
                else {
                    var result: Models.RelayModel[] = success.result;
                    deferred.resolve(result);
                }
            });

            return deferred.promise;
        }

        getTankLogs(tankId: string): ng.IPromise<Models.TankLogModel[]> {
            var apiUrl = "tank/log?tankId=" + tankId;

            var deferred = this.$q.defer<Models.TankLogModel[]>();

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "get"
            }, (error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /tank/log to get tank logs: " + error);
                    deferred.reject();
                }
                else {
                    var result: Models.TankLogModel[] = success.result;
                    deferred.resolve(result);
                }
            });

            return deferred.promise;
        }

        saveTankLog(newLog: Models.TankLogModel): ng.IPromise<void> {
            var deferred = this.$q.defer<void>();

            this.mobileService.invokeApi("tank/log", {
                body: newLog,
                method: "post"
            }, (error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /tank/log to post new tank log: " + error);
                    deferred.reject();
                }
                else {
                    deferred.resolve();
                }
            });

            return deferred.promise;
        }
    }

    angular.module("ngapp").service('dataService', DataService);
}