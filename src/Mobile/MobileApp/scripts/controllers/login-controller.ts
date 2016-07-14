namespace Submerged.Controllers {

    export class LoginController {

        loading: boolean;
        vm: LoginController = this;
        static $inject = ['mobileService', '$location', 'sharedService'];

        constructor(private mobileService: Submerged.Services.IMobileService, private $location: ng.ILocationService,
            private sharedService: Services.ISharedService) {

            this.ensureLogin().then(
                (() => {
                    return this.loadSettings.bind(this)();
                }).bind(this)
                , this.errorHandler).then(
                (() => {
                    navigator.splashscreen.hide();
                    this.redirect();
                }).bind(this)
                , this.errorHandler);
        }

        errorHandler(err): void {
            navigator.splashscreen.hide();
            console.log("Error occurred during login procedure: " + err);
        }        

        ensureLogin(): ng.IPromise<{}> {
            console.log("logincontroller.ensureLogin");
            return this.mobileService.login(true);
        }

        loadSettings(): ng.IPromise<{}> {
            console.log("logincontroller.loadSettings");
            return this.sharedService.init(this.mobileService);
        }

        redirect(): void {
            console.log('MAIN logged in, changing path');
            this.$location.path("/live");
            this.loading = false;
        }

        login(): void {
            this.mobileService.login();
        }
    }

    angular.module("ngapp").controller("LoginController", LoginController);
}