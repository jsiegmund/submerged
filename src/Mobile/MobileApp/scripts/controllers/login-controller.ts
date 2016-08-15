namespace Submerged.Controllers {

    export class LoginController {

        loading: boolean;
        vm: LoginController = this;
        //static $inject = ['mobileService', '$location', 'sharedService'];

        constructor(private mobileService: Submerged.Services.IMobileService, private $location: ng.ILocationService,
            private sharedService: Services.ISharedService, private dataService: Services.IDataService,
            private subscriptionService: Services.ISubscriptionService, private $q: ng.IQService) {

            console.log("Login: Starting login procedure, ensuring login");
            this.ensureLogin().then(() => {
                console.log("Login: Initializing services");
                return this.serviceInitialization();
            }, this.errorHandler).then(() => {
                console.log("Login: push registration initialization");
                navigator.splashscreen.hide();
                this.mobileService.initPushRegistration();
            }, this.errorHandler).finally(() => {
                console.log("Login: initialization done, redirecting");
                this.redirect();
            });

        }

        errorHandler(err): void {
            navigator.splashscreen.hide();
            console.log("Error occurred during login procedure: " + err);
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

        redirect(): void {
            this.$location.path("/live");
            this.loading = false;
        }

        login(): void {
            this.mobileService.login();
        }
    }

    angular.module("ngapp").controller("LoginController", LoginController);
}