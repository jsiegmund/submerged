/// <reference path="../shared/app.ts" />
interface Window {
    encodeURIComponent(url: string): string;
}

namespace Submerged.Controllers {
    "use strict";

    export class MainController {

        loading: boolean = false;

        constructor(private mobileService: Services.IMobileService, private $location: ng.ILocationService,
            private subscriptionService: Services.ISubscriptionService)   { 

        }

        logout(): void {
            this.loading = true;
            this.mobileService.logout().then(() => {
                // clear the stored subscription
                return this.subscriptionService.clear();
            }).then(() => {
                // reload the entire application after logging out for a fresh start
                window.location.replace("index.html");
            }, () => {
                this.loading = false;
            });
        }
    }

    angular.module("ngapp").controller("MainController", MainController);

}