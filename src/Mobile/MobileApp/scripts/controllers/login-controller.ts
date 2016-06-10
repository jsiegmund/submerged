namespace Submerged.Controllers {

    export class LoginController {

        loading: boolean;
        vm: LoginController = this;
        static $inject = ['mobileService', '$location', 'shared'];

        constructor(private mobileService: Submerged.Services.IMobileService, private $location: ng.ILocationService,
            private shared: Services.IShared) {

            this.ensureLogin().then(
                (() => {
                    return this.loadSettings.bind(this)();
                }).bind(this)
                , this.errorHandler).then(
                (() => {
                    this.redirect();
                }).bind(this)
                , this.errorHandler);
        }

        errorHandler(err): void {
            console.log("Error occurred during login procedure: " + err);
        }        

        ensureLogin(): ng.IPromise<{}> {
            console.log("logincontroller.ensureLogin");
            return this.mobileService.login(true);
        }

        loadSettings(): ng.IPromise<{}> {
            console.log("logincontroller.loadSettings");
            return this.shared.loadSettingsFromCloud(this.mobileService);
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

    angular.module("ngapp").controller("LoginContoller", LoginController);
}

//angular.module("ngapp").controller("LoginContoller", function (shared, mobileService, $scope, $location) {
//    var vm = this;

//    vm.loading = true;
//    vm.login = function () {
//        mobileService.login();
//    }

//    vm.init = function () {

//    }

//    vm.init();
//});
