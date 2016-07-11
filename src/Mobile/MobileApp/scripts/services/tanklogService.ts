namespace Submerged.Services {
    export interface ITankLogService {
        getTankLogs(tankId: string): ng.IPromise<Models.TankLogModel[]>;
        saveTankLog(newLog: Models.TankLogModel): ng.IPromise<{}>;
        deleteLog(tankId: string, logId: string): ng.IPromise<{}>;
        setSelectedLog(log: Models.TankLogModel): void;
        getSelectedLog(): Models.TankLogModel;
        getLogTypes(): {}[];
    }

    export class TankLogService implements ITankLogService {

        selectedLog: Models.TankLogModel;

        constructor(private mobileService: IMobileService, private $q: ng.IQService) {

        }

        getLogTypes(): {}[] {
            var allStates = 'Maintenance, Error';
            return allStates.split(/, +/g).map(function (state) {
                return {
                    value: state.toLowerCase(),
                    display: state
                };
            });
        }

        setSelectedLog(log: Models.TankLogModel): void {
            this.selectedLog = log;
        }

        getSelectedLog(): Models.TankLogModel {
            return this.selectedLog;
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

        deleteLog(tankId: string, logId: string): ng.IPromise<{}> {
            var deferred = this.$q.defer<Models.TankLogModel[]>();

            var apiUrl = "tank/log?tankId=" + tankId + "&logId=" + logId;

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "delete"
            }, (error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /tank/log to delete tank log: " + error);
                    deferred.reject();
                }
                else {
                    deferred.resolve();
                }
            });

            return deferred.promise;
        }
    }

    angular.module("ngapp").service('tankLogService', TankLogService);
}