namespace Submerged.Services {
    export interface IDataService {
        toggleMaintenance(deviceId: string, maintenanceMode: boolean): ng.IPromise<{}>;

        getSensors(deviceId: string): ng.IPromise<Models.SensorModel[]>;
        saveSensors(deviceId: string, sensors: Models.SensorModel[]): ng.IPromise<{}>;

        getRelays(deviceId: string): ng.IPromise<Models.RelayModel[]>;
        toggleRelay(deviceId: string, relayNumber: number, relayState: boolean): ng.IPromise<{}>;

        getTankLogs(tankId: string): ng.IPromise<Models.TankLogModel[]>;
        saveTankLog(newLog: Models.TankLogModel): ng.IPromise<{}>;
    }

    export class DataService implements IDataService {
        constructor(private mobileService: IMobileService, private $q: ng.IQService) {

        }

        saveSensors(deviceId: string, sensors: Models.SensorModel[]): ng.IPromise<{}> {
            var apiUrl = "sensors/save?deviceId=" + deviceId;
            var deferred = this.$q.defer<{}>();

            this.mobileService.invokeApi(apiUrl, {
                body: sensors,
                method: "post"
            }, ((error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /sensors/save to save the sensor configuration: " + error);
                    deferred.reject();
                }
                else {
                    deferred.resolve();
                }
            }).bind(this));

            return deferred.promise;
        }

        getSensors(deviceId: string): ng.IPromise<Models.SensorModel[]> {
            var apiUrl = "sensors?deviceId=" + deviceId;
            var deferred = this.$q.defer<Models.SensorModel[]>();

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, ((error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /data/sensors to get the sensor configuration: " + error);
                    deferred.reject();
                }
                else {
                    var result: Models.SensorModel[] = success.result;
                    deferred.resolve(result);
                }
            }).bind(this));

            return deferred.promise;
        }

        toggleMaintenance(deviceId: string, maintenanceMode: boolean): ng.IPromise<{}> {
            var apiUrl = "control/maintenance/toggle?deviceId=" + deviceId + "&inMaintenance=" + maintenanceMode;
            var deferred = this.$q.defer<Models.RelayModel[]>();

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, ((error, success) => {
                if (error) {
                    console.log("Error calling /control/maintenance/toggle to toggle maintenance state: " + error);
                    deferred.reject();
                }
                else {
                    deferred.resolve();
                }
            }).bind(this));


            return deferred.promise;
        }

        toggleRelay(deviceId: string, relayNumber: number, relayState: boolean): ng.IPromise<{}> {
            var apiUrl = "control/setrelay?deviceId=" + deviceId + "&relayNumber=" + relayNumber + "&state=" + relayState;
            var deferred = this.$q.defer<Models.RelayModel[]>();

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, ((error, success) => {
                if (error) {
                    console.log("Error calling /control/setrelay to toggle relay state: " + error);
                    deferred.reject();
                }
                else {
                    deferred.resolve();
                }
            }).bind(this));

            return deferred.promise;
        }

        getRelays(deviceId: string): ng.IPromise<Models.RelayModel[]> {
            var apiUrl = "control/relays?deviceId=" + deviceId;
            var deferred = this.$q.defer<Models.RelayModel[]>();

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, ((error, success) => {
                if (error) {
                    console.log("Error calling /control/relays to get relay configuration: " + error);
                    deferred.reject();
                }
                else {
                    var result: Models.RelayModel[] = success.result;
                    deferred.resolve(result);
                }
            }).bind(this));

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

        saveTankLog(newLog: Models.TankLogModel): ng.IPromise<{}> {
            var deferred = this.$q.defer<Models.TankLogModel[]>();

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