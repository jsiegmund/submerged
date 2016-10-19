/// <reference path="../shared/app.ts" />
namespace Submerged.Controllers {
    "use strict";

    export class BaseController {

        busy: boolean = false;

        constructor(private $mdToast: ng.material.IToastService, protected $q: ng.IQService) {

        }

        requireConfirmation(title: string, text: string): ng.IPromise<boolean> {
            var deferred = this.$q.defer<boolean>();

            navigator.notification.confirm(
                text,  // message
                (choice: number) => {
                    deferred.resolve(choice === 1);
                },              // callback to invoke with index of button pressed
                title            // title
            );

            return deferred.promise;
        }

        showSimpleToast(text: string) {
            this.$mdToast.show(
                this.$mdToast.simple()
                    .textContent(text)
                    .position("bottom center")
                    .hideDelay(3000)
            );
        };
    }
}