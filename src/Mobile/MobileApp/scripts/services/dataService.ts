/// <reference path="../shared/app.ts" />
namespace Submerged.Services {
    "use strict";

    export interface IDataService {
        toggleMaintenance(deviceId: string, maintenanceMode: boolean): ng.IPromise<void>;

        addDevice(device: Models.DeviceModel): ng.IPromise<Models.DeviceModel>;
        updateDevice(device: Models.DeviceModel): ng.IPromise<void>;
        deleteDevice(device: Models.DeviceModel): ng.IPromise<void>;

        getSensors(deviceId: string): ng.IPromise<Models.SensorModel[]>;
        addSensor(deviceId: string, sensor: Models.SensorModel): ng.IPromise<void>;
        updateSensor(deviceId: string, sensor: Models.SensorModel): ng.IPromise<void>;
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

        sendModuleCommand(deviceId: string, moduleName: string, command: any): ng.IPromise<any>;
    }

    export class DataService implements IDataService {
        
        constructor(private mobileService: IMobileService, private $q: ng.IQService, private sharedService: Services.ISharedService) {
            
        }

        getSubscription(): ng.IPromise<Models.SubscriptionModel> {
            var apiUrl = "subscription";
            return this.apiFunctionCall<Models.SubscriptionModel>(apiUrl, Statics.HTTP_VERBS.GET, null);
        }

        getData(rangeType: string, date: Date, deviceId: string): ng.IPromise<Models.AnalyticsDataModel> {
            var offset = this.sharedService.globalizationInfo.server_offset_seconds;
            var apiUrl = "data/" + rangeType + "?deviceId=" + deviceId + "&date=" + date.toISOString() + "&offset=" + offset;
            return this.apiFunctionCall<Models.AnalyticsDataModel>(apiUrl, Statics.HTTP_VERBS.GET, null);
        }

        getTelemetryLastThreeHours(deviceId: string, date: Date): ng.IPromise<Models.AnalyticsDataModel> {
            var offset = this.sharedService.globalizationInfo.server_offset_seconds;
            var apiUrl = "data/threehours?deviceId=" + deviceId + "&date=" + date.toISOString() + "&offset=" + offset;
            return this.apiFunctionCall<Models.AnalyticsDataModel>(apiUrl, Statics.HTTP_VERBS.GET, null);
        }

        getTelemetry(deviceId: string): ng.IPromise<Models.TelemetryModel> {
            var apiUrl = "data/latest?deviceId=" + deviceId;
            return this.apiFunctionCall<Models.TelemetryModel>(apiUrl, Statics.HTTP_VERBS.GET, null);
        }       

        getModules(deviceId: string): ng.IPromise<Models.ModuleModel[]> {
            var apiUrl = "modules?deviceId=" + deviceId;
            return this.apiFunctionCall<Models.ModuleModel[]>(apiUrl, Statics.HTTP_VERBS.GET, null);
        }

        addModule(deviceId: string, module: Models.ModuleModel): ng.IPromise<void> {
            var apiUrl = "modules?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.POST, module);
        }

        updateModule(deviceId: string, module: Models.ModuleModel): ng.IPromise<void> {
            var apiUrl = "modules?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.PUT, module);
        }

        deleteModule(deviceId: string, module: Models.ModuleModel): ng.IPromise<void> {
            var apiUrl = "modules?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.DELETE, module);
        }

        addDevice(device: Models.DeviceModel): ng.IPromise<Models.DeviceModel> {
            var apiUrl = "devices";
            return this.apiFunctionCall<Models.DeviceModel>(apiUrl, Statics.HTTP_VERBS.POST, device);
        }

        updateDevice(device: Models.DeviceModel): ng.IPromise<void> {
            var apiUrl = "devices";
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.PUT, device);
        }

        deleteDevice(device: Models.DeviceModel): ng.IPromise<void> {
            var apiUrl = "devices";
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.DELETE, device);
        }

        addSensor(deviceId: string, sensor: Models.SensorModel): ng.IPromise<void> {
            var apiUrl = "sensors?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.POST, sensor);
        }

        updateSensor(deviceId: string, sensor: Models.SensorModel): ng.IPromise<void> {
            var apiUrl = "sensors?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.PUT, sensor);
        }

        deleteSensor(deviceId: string, sensor: Models.SensorModel): ng.IPromise<void> {
            var apiUrl = "sensors?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.DELETE, sensor);
        }

        getSensors(deviceId: string): ng.IPromise<Models.SensorModel[]> {
            var apiUrl = "sensors?deviceId=" + deviceId;
            return this.apiFunctionCall<Models.SensorModel[]>(apiUrl, Statics.HTTP_VERBS.GET, null);
        }

        toggleMaintenance(deviceId: string, maintenanceMode: boolean): ng.IPromise<void> {
            var apiUrl = "control/maintenance/toggle?deviceId=" + deviceId + "&inMaintenance=" + maintenanceMode;
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.POST, null);
        }

        toggleRelay(deviceId: string, relayName: string, relayState: boolean): ng.IPromise<void> {
            var apiUrl = "control/setrelay?deviceId=" + deviceId + "&name=" + relayName + "&state=" + relayState;
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.POST, null);
        }

        getTanks(): ng.IPromise<Models.TankModel[]> {
            var apiUrl = "tanks";
            return this.apiFunctionCall<Models.TankModel[]>(apiUrl, Statics.HTTP_VERBS.GET, null);
        }

        addTank(tank: Models.TankModel): ng.IPromise<void> {
            var apiUrl = "tanks";
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.POST, tank);
        }

        updateTank(tank: Models.TankModel): ng.IPromise<void> {
            var apiUrl = "tanks";
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.PUT, tank);
        }

        deleteTank(tank: Models.TankModel): ng.IPromise<void> {
            var apiUrl = "tanks";
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.DELETE, tank);
        }

        addRelay(deviceId: string, relay: Models.RelayModel): ng.IPromise<void> {
            var apiUrl = "relays?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.POST, relay);
        }

        updateRelay(deviceId: string, relay: Models.RelayModel): ng.IPromise<void> {
            var apiUrl = "relays?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.PUT, relay);
        }

        deleteRelay(deviceId: string, relay: Models.RelayModel): ng.IPromise<void> {
            var apiUrl = "relays?deviceId=" + deviceId;
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.DELETE, relay);
        }

        getRelays(deviceId: string): ng.IPromise<Models.RelayModel[]> {
            var apiUrl = "control/relays?deviceId=" + deviceId;
            return this.apiFunctionCall<Models.RelayModel[]>(apiUrl, Statics.HTTP_VERBS.GET, null);
        }

        getTankLogs(tankId: string): ng.IPromise<Models.TankLogModel[]> {
            var apiUrl = "tank/log?tankId=" + tankId;
            return this.apiFunctionCall<Models.TankLogModel[]>(apiUrl, Statics.HTTP_VERBS.GET, null);
        }

        saveTankLog(newLog: Models.TankLogModel): ng.IPromise<void> {
            var apiUrl = "tank/log";
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.POST, null);
        }

        sendModuleCommand(deviceId: string, moduleName: string, command: any): ng.IPromise<void> {
            var apiUrl = `control/sendcommand?deviceId=${deviceId}&moduleName=${moduleName}`;
            return this.apiFunctionCall<void>(apiUrl, Statics.HTTP_VERBS.POST, command);
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
                    deferred.reject(error);
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