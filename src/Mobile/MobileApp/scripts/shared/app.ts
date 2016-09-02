"use strict";

angular.module("ngapp", ["ui.router", "ngMdIcons", "ngMaterial", "ngCordova", "ngCordova.plugins", "ngStorage", "angular-jwt", "pr.longpress"])

    .run(function ($rootScope: ng.IRootScopeService, $cordovaStatusbar: any, $location: ng.ILocationService,
        sharedService: Submerged.Services.ISharedService, subscriptionService: Submerged.Services.ISubscriptionService,
        menuService: Submerged.Services.IMenuService) {

        document.addEventListener("deviceready", onDeviceReady, false);

        function onDeviceReady() {
            hockeyapp.start(null, null, "c6076e8da2b24860aed146c667f32fa9");

            $cordovaStatusbar.overlaysWebView(false); // Always Show Status Bar
            window.plugins.orientationLock.lock("portrait");

            //document.addEventListener("backbutton", onBackButton, false);
            document.addEventListener("resume", onResume, false);
            $rootScope.$on('$stateChangeSuccess', changeTitle);
        }

        /* Using the title of the state to set the title of the page displayed in the menu bar.
           Menu service is intermediate between here and the actual menu controller
        */
        function changeTitle(event: ng.IAngularEvent, toState: any) {
            var newTitle = 'Submerged';

            if (toState.title)
                newTitle = toState.title;

            menuService.setTitle(newTitle);
            menuService.clearButtons();
            menuService.unhideMenu();
        }

        /* Hijack Android Back Button (You Can Set Different Functions for Each View by Checking the $state.current) */
        function onBackButton() {
            if ($location.path() == '/main') {
                //navigator.app.exitApp();
            } else {
                /*e.preventDefault();*/
            }
        }

        function onResume() {
            $rootScope.$broadcast('resume', null);
        }
    })

    .config(function ($mdThemingProvider) {
        $mdThemingProvider.theme('altTheme')
            .primaryPalette('purple');
    });