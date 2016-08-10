namespace Submerged.Directives {

    class SensorStockFloat implements ng.IDirective {
        public restrict = 'E'; //E = element, A = attribute, C = class, M = comment    
        public templateUrl = 'app/directives/sensor-stock-float.html';
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
            const directive = () => new SensorStockFloat();
            directive.$inject = [];
            return directive;
        }
    }

    angular.module('ngapp').directive('smSensorStockFloat', SensorStockFloat.factory());
}