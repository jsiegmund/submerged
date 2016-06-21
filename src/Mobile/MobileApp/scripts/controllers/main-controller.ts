"use strict";

namespace Submerged.Controllers {

    export class MainController {

        constructor(private mobileService: Services.IMobileService, private $location: ng.ILocationService) {
        }

        logout(): void {
            this.mobileService.logout().then(() => {
                this.$location.path("/login");
            });
        }
    }

    angular.module("ngapp").controller("MainController", MainController);

}