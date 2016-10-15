/// <reference path="../shared/app.ts" />
namespace Submerged.Directives {
    "use strict";

    interface ISensorGenericScope extends ng.IScope {
        formatSensorValue(sensor: Models.SensorModel, value: any): any;
        calculateSensorClass(sensor: Models.SensorModel): string;
        openDetails: any;
    }

    class SensorGeneric implements ng.IDirective {
        public restrict = 'E'; //E = element, A = attribute, C = class, M = comment    
        public templateUrl = 'app/directives/sensor-generic.html';
        public replace = true;
        public transclude = true;
        public controller = SensorController;
        public controllerAs = 'vm';
        public scope = {
            sensor: '='
        };

        constructor(private telemetryService: Services.ITelemetryService) {
        }

        link = ($scope: ISensorGenericScope, element, attrs, controller: ISensorController) => {
            $scope.formatSensorValue = controller.formatSensorValue;
            $scope.calculateSensorClass = controller.calculateSensorClass;
            $scope.openDetails = controller.openDetails;

            $scope.$watch(() => { return this.telemetryService.threeHoursData; }, (newValue, oldValue) => {
                if (newValue != undefined && newValue != null) {
                    for (var i = 0; i < newValue.serieLabels.length; i++) {
                        var sensorName = newValue.serieLabels[i];
                        var sensorModel: Models.SensorModel = this.telemetryService.sensors.firstOrDefault({ name: sensorName });

                        // this is a bit of an ugly solution because it depends on a div witht the proper id being present
                        // it works though and until I find something better this will do
                        controller.renderChart(newValue.dataLabels, newValue.dataSeries[i], sensorModel.name + "_chart");
                    }
                }
            });

        }

        static factory(): ng.IDirectiveFactory {
            const directive = (telemetryService) => new SensorGeneric(telemetryService);
            directive.$inject = ['telemetryService'];
            return directive;
        }

    }

    angular.module('ngapp').directive('smSensorGeneric', SensorGeneric.factory());;
}