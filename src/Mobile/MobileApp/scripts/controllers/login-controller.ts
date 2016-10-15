/// <reference path="../shared/app.ts" />
namespace Submerged.Controllers {
    "use strict";

    export class LoginController {

        loading: boolean;
        vm: LoginController = this;
        //static $inject = ['mobileService', '$location', 'sharedService'];

        constructor(private mobileService: Submerged.Services.IMobileService, private $location: ng.ILocationService,
            private sharedService: Services.ISharedService, private dataService: Services.IDataService,
            private subscriptionService: Services.ISubscriptionService, private $q: ng.IQService,
            private menuService: Services.IMenuService) {

            menuService.hideMenu();

            // initialize the mobile service and process local init afterwards 
            this.mobileService.init().finally(() => {

                if (this.mobileService.loggedIn()) {
                    console.log("Login: existing credentials found, attempting auto-login");
                    this.postLoginLogicAndRedirect();
                } else {
                    console.log("Login: no existing credentials found, displaying login view");
                    navigator.splashscreen.hide();
                    this.loading = false;
                }

            });
        }

        postLoginLogicAndRedirect(): void {
            console.log("Login: Initializing services");
            this.serviceInitialization().then(() => {
                console.log("Login: push registration initialization");
                navigator.splashscreen.hide();
                return this.mobileService.initPushRegistration();
            }, this.errorHandler).finally(() => {
                console.log("Login: initialization done, redirecting");
                this.$location.path("/live");
            });
            
        }

        errorHandler(err): void {
            console.log("Error occurred during login procedure: " + err);
            navigator.splashscreen.hide();
            this.loading = false;
        }        

        ensureLogin(): ng.IPromise<void> {
            console.log("logincontroller.ensureLogin");
            return this.mobileService.login(true);
        }

        serviceInitialization(): ng.IPromise<{}> {
            console.log("logincontroller.loadSettings");

            var q1 = this.sharedService.init();
            var q2 = this.subscriptionService.load(true);

            return this.$q.all([q1, q2]);
        }

        login(): void {
            this.loading = true;

            this.mobileService.login(true).then(() => {
                this.postLoginLogicAndRedirect();
            }, this.errorHandler);
        }
    }

    angular.module("ngapp").controller("LoginController", LoginController);
}