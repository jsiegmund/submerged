﻿namespace Submerged.Services {
    export interface IDataService {
        getTankLogs(tankId: string): ng.IPromise<Models.TankLogModel[]>;
        saveTankLog(newLog: Models.TankLogModel): ng.IPromise<{}>;
    }

    export class DataService implements IDataService {
        constructor(private mobileService: IMobileService, private $q: ng.IQService) {

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