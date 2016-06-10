"use strict";

angular.module("ngapp").config(["$stateProvider", "$urlRouterProvider", function ($stateProvider, $urlRouterProvider) {

    $urlRouterProvider.otherwise("/login");

    $stateProvider

        .state("login", {
            url: "/login",
            templateUrl: "app/views/login.html",
            title: "Submerged Login",
            controller: "LoginContoller",
            controllerAs: "vm"
        })

        .state("main", {
            url: "/main",
            templateUrl: "app/views/main.html",
            title: "Submerged Mobile",
            controller: "MainController",
            controllerAs: "vm"
        })

        .state("live", {
            url: "/live",
            templateUrl: "app/views/live.html",
            title: "Submerged Live",
            controller: "LiveController",
            controllerAs: "vm",
        })

        .state("analytics", {
            url: "/anaytics",
            templateUrl: "app/views/analytics.html",
            title: "Submerged Analytics",
            controller: "AnalyticsController",
            controllerAs: "vm",
            params: { 'tab': null, 'sensor': null }
        })

        .state("control", {
            url: "/control",
            templateUrl: "app/views/control.html",
            title: "Submerged Control",
            controller: "ControlController",
            controllerAs: "vm"
        })

        .state("settings", {
            url: "/settings",
            templateUrl: "app/views/settings.html",
            title: "Submerged Settings",
            controller: "SettingsController",
            controllerAs: "vm"
        });

}]);
