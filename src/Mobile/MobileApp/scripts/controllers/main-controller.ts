"use strict";

interface Window {
    encodeURIComponent(url: string): string;
}

namespace Submerged.Controllers {

    export class MainController {

        loading: boolean = false;

        constructor(private mobileService: Services.IMobileService, private $location: ng.ILocationService) {

        }

        logout(): void {
            this.loading = true;
            this.mobileService.logout().then(() => {
                // reload the entire application after logging out for a fresh start
                window.location.replace("index.html");
            }, () => {
                this.loading = false;
            });
        }
    }

    angular.module("ngapp").controller("MainController", MainController);

}