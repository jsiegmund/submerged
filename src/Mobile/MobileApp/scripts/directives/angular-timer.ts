/*
namespace Submerged.Directives {

    interface ITimerScope extends ng.IScope {
        startTimer();
        stopTimer();
    }

    export class TimerDirective implements ng.IDirective {
        public restrict = 'E';
        public scope = false;
        public replace = false;

        public link($scope: ITimerScope, $element: ng.IAugmentedJQuery, $timeout: ng.ITimeoutService, $compile: ng.ICompileService) {
            var timeoutId = null;

            var elem = $compile('<p>{{counter}}</p>')($scope);
            $element.append(elem);
        }

        static factory(): ng.IDirectiveFactory {
            const directive = () => new TimerDirective();
            directive.$inject = [];
            return directive;
        }
    }

    angular.module("ngapp").directive('timer', TimerDirective.factory());

    export class TimerController {
        constructor(private $scope: ITimerScope, $timeout: ng.ITimeoutService) {
            $scope.$on('timer-stopped', function (event, remaining) {
                if (remaining === 0) {
                    console.log('your time ran out!');
                }
            });
        }

        startTimer() {
            this.$scope.$broadcast('timer-start');
        };

        stopTimer() {
            this.$scope.$broadcast('timer-stop');
        };
    }

    angular.module('ngapp', []).controller('TimerCtrl', TimerController);
}*/