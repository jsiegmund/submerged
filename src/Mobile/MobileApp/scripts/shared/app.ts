"use strict";

angular.module("ngapp", ["ui.router", "ngMdIcons", "ngMaterial", "ngCordova", "ngCordova.plugins", "ngStorage", "TimerApp", "angular-jwt"])

    .run(function ($rootScope: ng.IRootScopeService, $cordovaStatusbar: any, $location: ng.ILocationService, shared: Submerged.Services.IShared) {

        document.addEventListener("deviceready", onDeviceReady, false);

        function onDeviceReady() {
            hockeyapp.start(null, null, "c6076e8da2b24860aed146c667f32fa9");

            $cordovaStatusbar.overlaysWebView(false); // Always Show Status Bar
            window.plugins.orientationLock.lock("portrait");

            //document.addEventListener("backbutton", onBackButton, false);
            document.addEventListener("resume", onResume, false);

            navigator.globalization.getDatePattern(globalizationSuccess, globalizationError);
        }

        function globalizationSuccess(pattern) {
            shared.globalizationInfo.utc_offset = pattern.utc_offset;
            shared.globalizationInfo.dst_offset = pattern.dst_offset;
        }

        function globalizationError(globalizationError) {
            console.log("Globalization error: " + globalizationError.message);
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