/// <reference path="../shared/app.ts" />
namespace Submerged.Services {
    "use strict";

    export interface ILedenetModuleService {
        selectPointInTime(point: Models.ModuleConfiguration.LedenetPointInTime);
        getSelectedPointInTime(): Models.ModuleConfiguration.LedenetPointInTime;
        savePoint(point: Models.ModuleConfiguration.LedenetPointInTime);
        testProgram();
    }

    export class LedenetModuleService implements ILedenetModuleService {

        selectedPoint: Models.ModuleConfiguration.LedenetPointInTime;

        constructor(private subscriptionService: ISubscriptionService, private dataService: IDataService,
            private sharedService: ISharedService) {
            
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

        testProgram() {
            var deviceId = this.subscriptionService.getSelectedDeviceID();
            var moduleName = this.subscriptionService.getSelectedModule().name;

            var command = {
                Action: "TestProgram"
            };

            this.dataService.sendModuleCommand(deviceId, moduleName, command);
        }
    }

    angular.module("ngapp").service('ledenetModuleService', LedenetModuleService);
}