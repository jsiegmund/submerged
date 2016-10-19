/// <reference path="../shared/app.ts" />
namespace Submerged.Controllers {
    "use strict";

    export class LiveController {

        loading: boolean = true;
        selectedTabIndex: number = 0;
        lastUpdated: number = 0;
        lastUpdatedText: string;
        deviceId: string
        timeoutId: any;

        modules: Submerged.Models.ModuleModel[];
        sensors: Submerged.Models.SensorModel[];


        constructor(
            private $scope: ng.IRootScopeService, private $timeout: ng.ITimeoutService,
            private dataService: Services.IDataService, private telemetryService: Services.ITelemetryService,
            private subscriptionService: Services.ISubscriptionService) {

            this.deviceId = subscriptionService.getSelectedDeviceID();

            $scope.$watch(() => { return this.selectedTabIndex; }, (newValue, oldValue) => {
                this.loadData();
            });

            $scope.$watch(() => {
                return telemetryService.lastLoadTime;
            }, (newValue, oldValue) => {
                if (newValue != undefined && newValue != null)
                    this.resetLastLoad(newValue);
            });

            $scope.$watch(() => {
                return telemetryService.sensors;
            }, (newValue, oldValue) => {
                if (newValue != undefined && newValue != null)
                    this.sensors = newValue;
            });

            this.$scope.$on("$destroy", () => {
                console.log("Stopping signalR hub since the user is leaving this view.");
                telemetryService.stopSignalR();
            });

            this.$scope.$on("resume", (event, data) => {
                console.log("Received resume event in live controller");

                // reload the data to freshen up
                this.loadTelemetry();

                // when the application gets resumed, we need to restart signalR
                telemetryService.startSignalR();
            });

        };

        loadData(): void {
            if (this.selectedTabIndex == 3) {
                this.loadModuleData();
            }
            else {
                this.loadTelemetry();
            }
        };

        loadTelemetry(): void {
            var dif = new Date().valueOf() - this.lastUpdated;
            var minutes = Math.floor(dif / (60000));

            if (minutes > 5) {
                this.loading = true;
                this.telemetryService.init().then(() => {
                    this.loading = false;
                }, () => {
                    this.loading = false;
                });
            }
        }

        filterStock = function (sensor: Models.SensorModel) {
            return sensor.sensorType === Statics.SENSORTYPES.STOCKFLOAT;
        }

        filterMoisture = function (sensor: Models.SensorModel) {
            return sensor.sensorType === Statics.SENSORTYPES.MOISTURE;
        }

        filterSensors = function (sensor: Models.SensorModel) {
            return sensor.sensorType != Statics.SENSORTYPES.STOCKFLOAT &&
                   sensor.sensorType != Statics.SENSORTYPES.MOISTURE &&
                   sensor.reading != null;
        }

        loadModuleData(): void {
            this.loading = true;
            this.dataService.getModules(this.deviceId).then(
                (modules) => { this.processModules(modules); }
            );
        };

        processModules = function (modules) {
            for (var module of modules) {
                if (module.status != "Connected")
                    module.cssClass = "npt-kpired";
                else
                    module.cssClass = "npt-kpigreen";
            }

            this.modules = modules;
            this.loading = false;
        }

        resetLastLoad(timestamp: number): void {
            if (this.timeoutId) {
                this.$timeout.cancel(this.timeoutId);
            }

            this.timeoutId = this.$timeout(this.onTimeout.bind(this), 1000);
            this.lastUpdated = timestamp;
        };

        stop(): void {
            this.$timeout.cancel(this.timeoutId);
            this.timeoutId = null;
        };

        onTimeout(): void {
            this.updateLastModified();
        }

        updateLastModified() {
            var dif: number;
            dif = new Date().valueOf() - this.lastUpdated;

            var seconds = Math.floor((dif / 1000) % 60);
            var minutes = Math.floor(((dif / (60000)) % 60));
            var hours = Math.floor(((dif / (3600000)) % 24));
            var days = Math.floor(((dif / (3600000)) / 24) % 30);
            var months = Math.floor(((dif / (3600000)) / 24 / 30) % 12);
            var years = Math.floor((dif / (3600000)) / 24 / 365);

            var text = null;

            if (years > 0)
                text = years.toString() + " years";
            else if (months > 0)
                text = months.toString() + " months";
            else if (days > 0)
                text = days.toString() + " days";
            else if (hours > 0)
                text = hours.toString() + " hours";
            else if (minutes > 0)
                text = minutes.toString() + " minutes";
            else
                text = seconds.toString() + " seconds";

            this.lastUpdatedText = text;
            this.timeoutId = this.$timeout(() => { this.onTimeout(); }, 1000);
        };
    }

    angular.module("ngapp").controller("LiveController", LiveController);
}