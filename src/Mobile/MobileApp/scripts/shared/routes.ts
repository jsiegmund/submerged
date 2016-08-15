"use strict";

angular.module("ngapp").config(["$stateProvider", "$urlRouterProvider", function ($stateProvider: ng.ui.IStateProvider, $urlRouterProvider: ng.ui.IUrlRouterProvider) {

    $urlRouterProvider.otherwise("/login");

    var defaultResolve = {
        SHARED_INIT: ['sharedService', function (sharedService: Submerged.Services.ISharedService) {
            console.log("Loading SharedService from route resolve");
            return sharedService.init().then(() => { console.log("ROUTES: LOADED SHAREDSERVICE"); });
        }],
        SUBSCRIPTION_INIT: ['subscriptionService', function (subscriptionService: Submerged.Services.ISubscriptionService) {
            console.log("Loading SubscriptionService from route resolve");
            return subscriptionService.load(false).then(() => { console.log("ROUTES: LOADED SUBSCRIPTION"); });
        }]
    };

    $stateProvider

        .state("login", <ng.ui.IState>{
            url: "/login",
            templateUrl: "app/views/login.html",
            title: "Submerged Login",
            resolve: defaultResolve,
            controller: "LoginController",
            controllerAs: "vm"
        })

        .state("main", <ng.ui.IState>{
            url: "/main",
            templateUrl: "app/views/main.html",
            title: "Submerged Mobile",
            resolve: defaultResolve,
            controller: "MainController",
            controllerAs: "vm"
        })

        .state("live", <ng.ui.IState>{
            url: "/live",
            templateUrl: "app/views/live.html",
            title: "Submerged Live",
            resolve: defaultResolve,
            controller: "LiveController",
            controllerAs: "vm",
        })

        .state("analytics", <ng.ui.IState>{
            url: "/analytics",
            templateUrl: "app/views/analytics.html",
            title: "Submerged Analytics",
            resolve: defaultResolve,
            controller: "AnalyticsController",
            controllerAs: "vm",
            params: { 'tab': null, 'sensor': null }
        })

        .state("control", <ng.ui.IState>{
            url: "/control",
            templateUrl: "app/views/control.html",
            title: "Submerged Control",
            resolve: defaultResolve,
            controller: "ControlController",
            controllerAs: "vm"
        })

        .state("settings", <ng.ui.IState>{
            url: "/settings",
            templateUrl: "app/views/settings.html",
            title: "Submerged Settings",
            resolve: defaultResolve,
            controller: "SettingsController",
            controllerAs: "vm"
        })

        .state("settings-device", <ng.ui.IState>{
            url: "/settings/device",
            templateUrl: "app/views/settings-device.html",
            title: "Submerged Settings",
            resolve: defaultResolve,
            controller: "SettingsController",
            controllerAs: "vm"
        })

        .state("settings-sensor", <ng.ui.IState>{
            url: "/settings/sensor",
            templateUrl: "app/views/settings-sensor.html",
            title: "Submerged Settings",
            resolve: defaultResolve,
            controller: "SettingsSensorController",
            controllerAs: "vm"
        })

        .state("settings-relay", <ng.ui.IState>{
            url: "/settings/relay",
            templateUrl: "app/views/settings-relay.html",
            title: "Submerged Settings",
            resolve: defaultResolve,
            controller: "SettingsRelayController",
            controllerAs: "vm"
        })

        .state("settings-module", <ng.ui.IState>{
            url: "/settings/module",
            templateUrl: "app/views/settings-module.html",
            title: "Submerged Settings",
            resolve: defaultResolve,
            controller: "SettingsModuleController",
            controllerAs: "vm"
        })

        .state("settings-tank", <ng.ui.IState>{
            url: "/settings/tank",
            templateUrl: "app/views/settings-tank.html",
            title: "Submerged Settings",
            resolve: defaultResolve,
            controller: "SettingsTankController",
            controllerAs: "vm"
        })

        .state("tanklog", <ng.ui.IState>{
            url: "/tanklog",
            templateUrl: "app/views/tanklog.html",
            title: "Submerged Tank Log",
            resolve: defaultResolve,
            controller: "TankLogController",
            controllerAs: "vm"           
        })

        .state("tanklog-new", <ng.ui.IState>{
            url: "/tanklog/new",
            templateUrl: "app/views/tanklog-new.html",
            title: "New Tank Log",
            resolve: defaultResolve,
            controller: "TankLogController",
            controllerAs: "vm"
        })

        .state("tanklog-detail", <ng.ui.IState>{
            url: "/tanklog/detail",
            templateUrl: "app/views/tanklog-detail.html",
            title: "Tanklog",
            resolve: defaultResolve,
            controller: "TankLogController",
            controllerAs: "vm"
        })

}]);
