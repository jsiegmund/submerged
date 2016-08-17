namespace Submerged.Services {
    export interface IDataService {
        toggleMaintenance(deviceId: string, maintenanceMode: boolean): ng.IPromise<void>;

        getSensors(deviceId: string): ng.IPromise<Models.SensorModel[]>;
        addSensor(deviceId: string, sensor: Models.SensorModel): ng.IPromise<void>;
        updateSensor(deviceId: string, sensor: Models.SensorModel): ng.IPromise<void>;
        updateSensors(deviceId: string, sensors: Models.SensorModel[]): ng.IPromise<void>;
        deleteSensor(deviceId: string, sensor: Models.SensorModel): ng.IPromise<void>;

        getRelays(deviceId: string): ng.IPromise<Models.RelayModel[]>;
        addRelay(deviceId: string, relay: Models.RelayModel): ng.IPromise<void>;
        updateRelay(deviceId: string, relay: Models.RelayModel): ng.IPromise<void>;
        deleteRelay(deviceId: string, relay: Models.RelayModel): ng.IPromise<void>;
        toggleRelay(deviceId: string, relayName: string, relayState: boolean): ng.IPromise<void>;

        getTankLogs(tankId: string): ng.IPromise<Models.TankLogModel[]>;
        saveTankLog(newLog: Models.TankLogModel): ng.IPromise<void>;

        getModules(deviceId: string): ng.IPromise<Models.ModuleModel[]>;
        addModule(deviceId: string, module: Models.ModuleModel): ng.IPromise<void>;
        updateModule(deviceId: string, module: Models.ModuleModel): ng.IPromise<void>;
        deleteModule(deviceId: string, module: Models.ModuleModel): ng.IPromise<void>;

        getTanks(): ng.IPromise<Models.TankModel[]>;
        addTank(tank: Models.TankModel): ng.IPromise<void>;
        updateTank(tank: Models.TankModel): ng.IPromise<void>;
        deleteTank(tank: Models.TankModel): ng.IPromise<void>;

        getTelemetry(deviceId: string): ng.IPromise<any>;
        getTelemetryLastThreeHours(deviceId: string, date: Date): ng.IPromise<any>;

        getData(rangeType: string, date: Date, deviceId: string): ng.IPromise<Models.AnalyticsDataModel>;

        getSubscription(): ng.IPromise<Models.SubscriptionModel>;  
    }

    export class DataService implements IDataService {
        
        constructor(private mobileService: IMobileService, private $q: ng.IQService, private sharedService: Services.ISharedService) {
            
        }

        getSubscription(): ng.IPromise<Models.SubscriptionModel> {
            var apiUrl = "subscription";
            return this.apiFunctionCall<Models.SubscriptionModel>(apiUrl, "get", null);
        }

        getData(rangeType: string, date: Date, deviceId: string): ng.IPromise<Models.AnalyticsDataModel> {
            var offset = this.sharedService.globalizationInfo.server_offset_seconds;
            var apiUrl = "data/" + rangeType + "?deviceId=" + deviceId + "&date=" + date.toISOString() + "&offset=" + offset;
            return this.apiFunctionCall<Models.AnalyticsDataModel>(apiUrl, "get", null);
        }

        getTelemetryLastThreeHours(deviceId: string, date: Date): ng.IPromise<Models.AnalyticsDataModel> {
            var offset = this.sharedService.globalizationInfo.server_offset_seconds;
            var apiUrl = "data/threehours?deviceId=" + deviceId + "&date=" + date.toISOString() + "&offset=" + offset;
            return this.apiFunctionCall<Models.AnalyticsDataModel>(apiUrl, "get", null);
        }

        getTelemetry(deviceId: string): ng.IPromise<Models.TelemetryModel> {
            var apiUrl = "data/latest?deviceId=" + deviceId;
            return this.apiFunctionCall<Models.TelemetryModel>(apiUrl, "get", null);
        }       

        getModules(deviceId: string): ng.IPromise<Models.ModuleModel[]> {
            var apiUrl = "modules?deviceId=" + deviceId;
            return this.apiFunctionCall<Models.ModuleModel[]>(apiUrl, "get", null);
        }

        addModule(deviceId: string, module: Models.ModuleModel): ng.IPromise<void> {
            var apiUrl = "modules?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, "post", module);
        }

        updateModule(deviceId: string, module: Models.ModuleModel): ng.IPromise<void> {
            var apiUrl = "modules?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, "put", module);
        }

        deleteModule(deviceId: string, module: Models.ModuleModel): ng.IPromise<void> {
            var apiUrl = "modules?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, "post", module);
        }

        addSensor(deviceId: string, sensor: Models.SensorModel): ng.IPromise<void> {
            var apiUrl = "sensors?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, "post", sensor);
        }

        updateSensor(deviceId: string, sensor: Models.SensorModel): ng.IPromise<void> {
            var apiUrl = "sensors?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, "put", sensor);
        }

        updateSensors(deviceId: string, sensors: Models.SensorModel[]): ng.IPromise<void> {
            var apiUrl = "sensors/save?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, "put", sensors);
        }

        deleteSensor(deviceId: string, sensor: Models.SensorModel): ng.IPromise<void> {
            var apiUrl = "sensors?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, "delete", sensor);
        }

        getSensors(deviceId: string): ng.IPromise<Models.SensorModel[]> {
            var apiUrl = "sensors?deviceId=" + deviceId;
            return this.apiFunctionCall<Models.SensorModel[]>(apiUrl, "get", null);
        }

        toggleMaintenance(deviceId: string, maintenanceMode: boolean): ng.IPromise<void> {
            var apiUrl = "control/maintenance/toggle?deviceId=" + deviceId + "&inMaintenance=" + maintenanceMode;
            return this.apiFunctionCall<void>(apiUrl, "post", null);
        }

        toggleRelay(deviceId: string, relayName: string, relayState: boolean): ng.IPromise<void> {
            var apiUrl = "control/setrelay?deviceId=" + deviceId + "&name=" + relayName + "&state=" + relayState;
            return this.apiFunctionCall<void>(apiUrl, "post", null);
        }

        getTanks(): ng.IPromise<Models.TankModel[]> {
            var apiUrl = "tanks";
            return this.apiFunctionCall<Models.TankModel[]>(apiUrl, "get", null);
        }

        addTank(tank: Models.TankModel): ng.IPromise<void> {
            var apiUrl = "tanks";
            return this.apiFunctionCall<void>(apiUrl, "post", tank);
        }

        updateTank(tank: Models.TankModel): ng.IPromise<void> {
            var apiUrl = "tanks";
            return this.apiFunctionCall<void>(apiUrl, "put", tank);
        }

        deleteTank(tank: Models.TankModel): ng.IPromise<void> {
            var apiUrl = "tanks";
            return this.apiFunctionCall<void>(apiUrl, "delete", tank);
        }

        addRelay(deviceId: string, relay: Models.RelayModel): ng.IPromise<void> {
            var apiUrl = "relays?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, "post", relay);
        }

        updateRelay(deviceId: string, relay: Models.RelayModel): ng.IPromise<void> {
            var apiUrl = "relays?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, "put", relay);
        }

        deleteRelay(deviceId: string, relay: Models.RelayModel): ng.IPromise<void> {
            var apiUrl = "relays?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, "delete", relay);
        }

        getRelays(deviceId: string): ng.IPromise<Models.RelayModel[]> {
            var apiUrl = "control/relays?deviceId=" + deviceId;
            return this.apiFunctionCall<Models.RelayModel[]>(apiUrl, "get", null);
        }

        getTankLogs(tankId: string): ng.IPromise<Models.TankLogModel[]> {
            var apiUrl = "tank/log?tankId=" + tankId;
            return this.apiFunctionCall<Models.TankLogModel[]>(apiUrl, "get", null);
        }

        saveTankLog(newLog: Models.TankLogModel): ng.IPromise<void> {
            var apiUrl = "tank/log";
            return this.apiFunctionCall<void>(apiUrl, "post", null);
        }


        private apiFunctionCall<T>(apiUrl: string, method: string, body: any): ng.IPromise<T> {
            var deferred = this.$q.defer<T>();

            this.mobileService.invokeApi(apiUrl, {
                body: angular.toJson(body),
                method: method
            }, (error, success) => {
                if (error) {
                    // do 
                    var errorMessage = `Error calling ${apiUrl} on the server. Details: ${error}`;
                    console.log(errorMessage);
                    deferred.reject();
                }
                else {
                    deferred.resolve(success.result);
                }
            });

            return deferred.promise;
        }
    }

    angular.module("ngapp").service('dataService', DataService);
}