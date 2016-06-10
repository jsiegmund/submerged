interface ITimerScope extends ng.IScope {
    startTimer();
    stopTimer();
}

angular.module('TimerApp', [])
    .controller('TimerCtrl', function ($scope: ITimerScope, $timeout: ng.ITimeoutService) {
        $scope.startTimer = function () {
            $scope.$broadcast('timer-start');
        };
        $scope.stopTimer = function () {
            $scope.$broadcast('timer-stop');
        };
        $scope.$on('timer-stopped', function (event, remaining) {
            if (remaining === 0) {
                console.log('your time ran out!');
            }
        });
    })
    .directive('timer', function ($compile: ng.ICompileService) {
        return {
            restrict: 'E',
            scope: false,
            replace: false,
            controller: function ($scope: ITimerScope, $element: ng.IAugmentedJQuery, $timeout: ng.ITimeoutService) {
                var timeoutId = null;

                var elem = $compile('<p>{{counter}}</p>')($scope);
                $element.append(elem);
            }
        };
    });