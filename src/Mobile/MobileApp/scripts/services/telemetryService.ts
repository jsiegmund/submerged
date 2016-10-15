/// <reference path="../shared/app.ts" />
namespace Submerged.Services {
    "use strict";

    export interface ITelemetryService {

        sensors: Models.SensorModel[];
        threeHoursData: Models.AnalyticsDataModel;
        lastLoadTime: number;

        init(): ng.IPromise<void>;
        loadTelemetry(): ng.IPromise<void>;

        stopSignalR(): void;
        startSignalR(): void;
    }

    export class TelemetryService implements ITelemetryService {

        public sensors: Models.SensorModel[];
        public threeHoursData: Models.AnalyticsDataModel;
        public lastLoadTime: number;

        private deviceId: string;

        constructor(private dataService: IDataService, private sharedService: ISharedService, private signalRService: ISignalRService,
            private $q: ng.IQService, private subscriptionService: Services.ISubscriptionService) {
            this.deviceId = subscriptionService.getSelectedDeviceID();
        }

        init(): ng.IPromise<void> {
            return this.loadSensors().then(
                () => {
                    return this.loadTelemetry();
                }
            );
        }

        loadTelemetry(): ng.IPromise<void> {
            var q1 = this.loadLatestTelemetry();
            var q2 = this.loadLastThreeHours();

            var deferred = this.$q.defer<void>();

            this.$q.all([q1, q2]).then(
                () => {
                    deferred.resolve();
                },
                () => {
                    deferred.reject();
                }
            );

            return deferred.promise;
        }

        loadSensors(): ng.IPromise<void> {
            var deferred = this.$q.defer<void>();

            // get the settings stored in local storage; when empty refresh from cloud
            this.dataService.getSensors(this.deviceId).then(
                (sensors) => {
                    this.sensors = sensors;
                    deferred.resolve();

                    this.processSensors(sensors);      // process the last known data for display
                },
                () => { deferred.reject(); }
            );

            return deferred.promise;
        }

        loadLastThreeHours(): ng.IPromise<void> {
            var deferred = this.$q.defer<void>();
            var date = new Date();

            this.dataService.getTelemetryLastThreeHours(this.deviceId, date).then(
                (data) => {
                    this.processThreeHours(data);
                    deferred.resolve();
                },
                () => { deferred.reject(); }
            );

            return deferred.promise;
        }

        processThreeHours = (data: Models.AnalyticsDataModel): void => {
            this.threeHoursData = data;
        }


        loadLatestTelemetry(): ng.IPromise<void> {
            var deferred = this.$q.defer<void>();

            // get the latest available data record to show untill it's updated
            this.dataService.getTelemetry(this.deviceId).then(
                (telemetry) => {
                    this.processTelemetry(telemetry);       // process the last known data for display
                    this.startSignalR();         // start signalR when the data is received 

                    deferred.resolve();
                },
                () => { deferred.reject(); }
            );

            return deferred.promise;
        }

        processSensors(sensors: Models.SensorModel[]): void {
            this.sensors = sensors;
        }

        processTelemetry = (telemetry: Models.TelemetryModel): void => {
            // TODO: temporary fix until proper pubsub style signalR has been implemented 
            if (telemetry.deviceId != this.deviceId)
                return;

            // when the timestamp is set (blob stored data); use it, otherwise timestamp = now
            if (telemetry.eventEnqueuedUTCTime != null)
                this.lastLoadTime = telemetry.eventEnqueuedUTCTime.valueOf()
            else
                this.lastLoadTime = new Date().valueOf();

            for (var sensorItem of telemetry.sensorData) {
                // find a sensor by the name of the property
                var sensor: Models.SensorModel = this.sensors.firstOrDefault({ name: sensorItem.sensorName });
                if (sensor != null) {
                    sensor.reading = sensorItem.value;
                }
            }
        };

        stopSignalR(): void {
            jQuery.connection.hub.stop();
        }

        startSignalR(): void {
            this.signalRService.init(this.processTelemetry);
        };
    }


    angular.module("ngapp").service('telemetryService', TelemetryService);
}