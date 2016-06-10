interface IMenuScope extends ng.IScope {
    toggle();
    navigate(route: string);
    title: string;
}

angular.module("ngapp").controller("MenuController", function (shared: Submerged.Services.IShared, $state: ng.ui.IStateService, $scope: IMenuScope,
    $rootScope: ng.IRootScopeService, $timeout: ng.ITimeoutService, $mdSidenav) {

    var toggle = function () {
        $mdSidenav("left").toggle();
    }

    var navigate = function (route) {
        $state.go(route);
        this.toggle();
    }

    $scope.toggle = toggle;
    $scope.navigate = navigate;

    var listener = function (event, toState) {

        var title = 'Submerged';
        if (toState.title) title = toState.title;

        $timeout(function () {
            $scope.title = title;
        }, 0, false);
    };

    $rootScope.$on('$stateChangeSuccess', listener);
});