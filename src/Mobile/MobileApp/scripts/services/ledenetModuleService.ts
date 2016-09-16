namespace Submerged.Services {

    export interface ILedenetModuleService {
        selectPointInTime(point: Models.ModuleConfiguration.LedenetPointInTime);
        getSelectedPointInTime(): Models.ModuleConfiguration.LedenetPointInTime;
        savePoint(point: Models.ModuleConfiguration.LedenetPointInTime);
    }

    export class LedenetModuleService implements ILedenetModuleService {

        selectedPoint: Models.ModuleConfiguration.LedenetPointInTime;

        constructor(private subscriptionService: ISubscriptionService) {
            
        }                

        savePoint(point: Models.ModuleConfiguration.LedenetPointInTime) {
            var config = this.subscriptionService.getSelectedModule().configuration;

            if (!config.pointsInTime)
                config.pointsInTime = new Array<Models.ModuleConfiguration.LedenetPointInTime>();
            
            // try to find a point in time with the same time
            var existing = config.pointsInTime.firstOrDefault(x => x.time == point.time);

            if (existing) {
                var indexOf = config.pointsInTime.indexOf(existing);
                config.pointsInTime.splice(indexOf, 1, point);
            }
            else {
                config.pointsInTime.push(point);
            }
        }

        getSelectedPointInTime(): Models.ModuleConfiguration.LedenetPointInTime {
            if (!this.selectedPoint)
                return new Models.ModuleConfiguration.LedenetPointInTime;
            else
                return this.selectedPoint;
        }

        selectPointInTime(point: Models.ModuleConfiguration.LedenetPointInTime) {
            this.selectedPoint = point;
        }
    }

    angular.module("ngapp").service('ledenetModuleService', LedenetModuleService);
}