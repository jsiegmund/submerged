namespace Submerged.Controllers {

    export class TankLogController {

        private logs: Models.TankLogModel[];
        private newLog: Models.TankLogModel;
        private selectedLog: Models.TankLogModel;
        private loading: boolean = true;

        private logTypes: {}[];

        constructor(private shared: Services.IShared, private tankLogService: Services.ITankLogService, private $location: ng.ILocationService) {
            var tank = shared.settings.subscription.tanks.first();

            this.newLog = new Models.TankLogModel;
            this.newLog.tankId = tank.id;

            this.tankLogService.getTankLogs(tank.id).then((result) => {
                this.logs = result;
                this.loading = false;
            }, (error) => {
                this.loading = false;
            });

            this.logTypes = this.loadAllLogTypes();
            this.selectedLog = this.tankLogService.getSelectedLog();
        }

        deleteLog(logId: string) {
            var tank = this.shared.settings.subscription.tanks.first();
            this.tankLogService.deleteLog(tank.id, logId);
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

        openDetails(log: Models.TankLogModel): void {
            this.tankLogService.setSelectedLog(log);
            this.$location.path("/tanklog/detail");
        }
    }

    angular.module("ngapp").controller("TankLogController", TankLogController);
}