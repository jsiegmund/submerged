/// <reference path="../shared/app.ts" />
namespace Submerged.Directives {
    "use strict";

    class SensorMoisture implements ng.IDirective {
        public restrict = 'E'; //E = element, A = attribute, C = class, M = comment    
        public templateUrl = 'app/directives/sensor-moisture.html';
        public replace = true;
        public transclude = true;
        public controller = SensorController;
        public controllerAs = 'vm';
        public scope = {
            sensor: '=',
        };

        link = ($scope, element, attrs, controller: ISensorController) => {
            $scope.formatSensorValue = controller.formatSensorValue;
            $scope.calculateSensorClass = controller.calculateSensorClass;
        }

        static factory(): ng.IDirectiveFactory {
            const directive = () => new SensorMoisture();
            directive.$inject = [];
            return directive;
        }

    }

    angular.module('ngapp').directive('smSensorMoisture', SensorMoisture.factory());
}