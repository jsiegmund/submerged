namespace Submerged.Controllers {

    export class TankLogController {

        private logs: Models.TankLogModel[];

        constructor(private shared: Services.IShared, private dataService: Services.IDataService) {
            var tank = shared.settings.subscription.tanks.first();            
            this.dataService.getTankLogs(tank.id).then((result) => {
                this.logs = result;
            });
        }
    }

    angular.module("ngapp").controller("TankLogController", TankLogController);
}