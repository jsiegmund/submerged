/// <reference path="../shared/app.ts" />
namespace Submerged.Controllers {
    "use strict";

    export class TankLogController extends BaseController {

        private logs: Models.TankLogDisplayModel[];
        private newLog: Models.TankLogDisplayModel;
        private selectedLog: Models.TankLogModel;
        private loading: boolean = true;

        private logTypes: {}[];

        constructor(private sharedService: Services.ISharedService, private tankLogService: Services.ITankLogService, private $location: ng.ILocationService,
            private menuService: Services.IMenuService, $mdToast: ng.material.IToastService, private $q: ng.IQService, subscriptionService: Services.ISubscriptionService) {

            super($mdToast);

            subscriptionService.load().then((subscription) => {
                var tank = subscription.tanks.first();
                this.newLog = new Models.TankLogDisplayModel;
                this.newLog.tankId = tank.name;

                this.tankLogService.getTankLogs(tank.name).then((result) => {
                    this.logs = <Models.TankLogDisplayModel[]>result;
                    this.loading = false;
                }, (error) => {
                    this.loading = false;
                });
            });

            this.logTypes = this.loadAllLogTypes();
            this.selectedLog = this.tankLogService.getSelectedLog();
        }

        logSelectionChanged() {
            var selected = this.logs.any({ selected: true });

            if (selected && this.menuService.getButtons().length == 0) {
                var deleteButton = new Services.CommandButton();
                deleteButton.svgSrc = 'icons/ic_delete_24px.svg';
                deleteButton.clickHandler = this.deleteLogs;
                deleteButton.label = 'Delete';
                deleteButton.owner = this;

                this.menuService.setButtons([deleteButton]);
            }
            else if (!selected && this.menuService.getButtons().length > 0) {
                this.menuService.setButtons([]);
            }
        }

        toggleLogSelected(log: Models.TankLogDisplayModel) {
            log.selected = !log.selected;
            this.logSelectionChanged();
        };

        logClicked(log: Models.TankLogDisplayModel, $event: ng.IAngularEvent): void {
            var selected = this.logs.any({ selected: true });

            if (!selected)
            {
                this.tankLogService.setSelectedLog(log);
                this.$location.path("/tanklog/detail");
            }                
            else
                this.toggleLogSelected(log);                
        }

        deleteLogs() {
            var selected = this.logs.where({ selected: true });
            var promises = [];

            for (var log of selected) {
                promises.push(this.deleteLog(log));
            }

            this.$q.all(promises).then((results) => {
                var failed = results.any(function (a) { return a === false });

                // display a toast to inform the user of the outcome
                if (!failed && results.length > 1)
                    this.showSimpleToast("Logs have been deleted.");
                else if (!failed && results.length === 1)
                    this.showSimpleToast("Log has been deleted.");
                else
                    this.showSimpleToast("Something went wrong.");

                // call logSelectedChanged to remove the delete button again
                this.logSelectionChanged();
            }, () => {
                this.logSelectionChanged();
            });
        }

        deleteLog(log: Models.TankLogDisplayModel): ng.IPromise<boolean> {
            var deferred = this.$q.defer();

            this.tankLogService.deleteLog(log.tankId, log.logId).then(() => {
                // success = delete log entry from local list
                var index = this.logs.indexOf(log);
                this.logs.splice(index, 1);

                deferred.resolve(true);
            }, () => {
                deferred.resolve(false);
            });

            return deferred.promise;
        }

        getLogIcon(logType: string): string {
            if (logType === "maintenance")
                return 'icons/ic_build_24px.svg';
            else if (logType === "error")
                return 'icons/ic_error_24px.svg';
            else
                return 'icons/ic_comment_black_24px.svg';
        }

        loadAllLogTypes(): {}[] {
            return this.tankLogService.getLogTypes();
        }

        querySearch(query): any {
            var results = query ? this.logTypes.filter(this.createFilterFor(query)) : this.logTypes,
                deferred;
            return results;
        }


        /**
         * Create filter function for a query string
         */
        createFilterFor(query): (state: any) => boolean {
            var lowercaseQuery = angular.lowercase(query);
            return function filterFn(state) {
                return (state.value.indexOf(lowercaseQuery) === 0);
            };
        }

        gotoNew(): void {
            this.$location.path("/tanklog/new");
        }

        cancelNew(): void {
            this.$location.path("/tanklog");
        }

        saveNew(): void {
            this.tankLogService.saveTankLog(this.newLog).then(() => {
                this.$location.path("/tanklog");
            });
        }
    }

    angular.module("ngapp").controller("TankLogController", TankLogController);
}