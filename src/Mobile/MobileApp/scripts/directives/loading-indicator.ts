angular.module('ngapp').directive('smLoadingIndicator', function () {
    return {
        restrict: 'EA', //E = element, A = attribute, C = class, M = comment    
        templateUrl: 'app/directives/loading-indicator.html',
        replace: true,
        transclude: true,
        scope: {
            displayText: '=smText'
        },
        link: function ($scope, element, attrs) {
        }
    };
});