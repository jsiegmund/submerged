namespace Submerged.Directives {

    class SensorGeneric implements ng.IDirective {
        public restrict = 'E'; //E = element, A = attribute, C = class, M = comment    
        public templateUrl = 'app/directives/sensor-generic.html';
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
            $scope.eventHandler = attrs.ngClick;
        }

        static factory(): ng.IDirectiveFactory {
            const directive = () => new SensorGeneric();
            directive.$inject = [];
            return directive;
        }

    }

    angular.module('ngapp').directive('smSensorGeneric', SensorGeneric.factory());
}