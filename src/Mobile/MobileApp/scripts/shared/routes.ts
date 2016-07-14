﻿"use strict";

angular.module("ngapp").config(["$stateProvider", "$urlRouterProvider", function ($stateProvider: ng.ui.IStateProvider, $urlRouterProvider: ng.ui.IUrlRouterProvider) {

    $urlRouterProvider.otherwise("/login");

    $stateProvider

        .state("login", <ng.ui.IState>{
            url: "/login",
            templateUrl: "app/views/login.html",
            title: "Submerged Login",
            controller: "LoginController",
            controllerAs: "vm"
        })

        .state("main", <ng.ui.IState>{
            url: "/main",
            templateUrl: "app/views/main.html",
            title: "Submerged Mobile",
            controller: "MainController",
            controllerAs: "vm"
        })

        .state("live", <ng.ui.IState>{
            url: "/live",
            templateUrl: "app/views/live.html",
            title: "Submerged Live",
            controller: "LiveController",
            controllerAs: "vm",
        })

        .state("analytics", <ng.ui.IState>{
            url: "/anaytics",
            templateUrl: "app/views/analytics.html",
            title: "Submerged Analytics",
            controller: "AnalyticsController",
            controllerAs: "vm",
            params: { 'tab': null, 'sensor': null }
        })

        .state("control", <ng.ui.IState>{
            url: "/control",
            templateUrl: "app/views/control.html",
            title: "Submerged Control",
            controller: "ControlController",
            controllerAs: "vm"
        })

        .state("settings", <ng.ui.IState>{
            url: "/settings",
            templateUrl: "app/views/settings.html",
            title: "Submerged Settings",
            controller: "SettingsController",
            controllerAs: "vm"
        })

        .state("tanklog", <ng.ui.IState>{
            url: "/tanklog",
            templateUrl: "app/views/tanklog.html",
            title: "Submerged Tank Log",
            controller: "TankLogController",
            controllerAs: "vm"           
        })

        .state("tanklog-new", <ng.ui.IState>{
            url: "/tanklog/new",
            templateUrl: "app/views/tanklog-new.html",
            title: "New Tank Log",
            controller: "TankLogController",
            controllerAs: "vm"
        })

        .state("tanklog-detail", <ng.ui.IState>{
            url: "/tanklog/detail",
            templateUrl: "app/views/tanklog-detail.html",
            title: "Tanklog",
            controller: "TankLogController",
            controllerAs: "vm"
        })

}]);
