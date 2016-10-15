/// <reference path="../shared/app.ts" />
namespace Submerged.Directives {
    "use strict";

    export class LoadingIndicatorDirective implements ng.IDirective {
        public restrict = 'EA'; //E = element, A = attribute, C = class, M = comment    
        public templateUrl = 'app/directives/loading-indicator.html';
        public replace = true;
        public transclude = true;
        public scope = {
            displayText: '=smText'
        };

        link($scope, element, attrs) {

        }

        static factory(): ng.IDirectiveFactory {
            const directive = () => new LoadingIndicatorDirective();
            directive.$inject = [];
            return directive;
        }
    }

    angular.module('ngapp').directive('smLoadingIndicator', LoadingIndicatorDirective.factory());
}