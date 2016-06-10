namespace Submerged.Controllers {
    export class ControlController {

        relays: Models.RelayModel[];

        constructor(private shared: Services.IShared, private mobileService: Services.IMobileService, private $mdToast: ng.material.IToastService,
            private $state: ng.ui.IStateService) {

            var apiUrl = "control/relays?deviceId=" + this.shared.deviceInfo.deviceId;

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, ((error, success) => {
                if (!error) {
                    // do nothing
                    this.processRelays(success.result);
                }
            }).bind(this));
        }

        processRelays(relays: Models.RelayModel[]) {
            this.relays = relays;
        }

         showSimpleToast(text: string) {
            this.$mdToast.show(
                this.$mdToast.simple()
                    .textContent(text)
                    .position("top right")
                    .hideDelay(3000)
            );
        };

        toggle(relayNumber: number, relayState: boolean) {
            var apiUrl = "control/setrelay?deviceId=" + this.shared.deviceInfo.deviceId + "&relayNumber=" + relayNumber + "&state=" + relayState;
            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, ((error, success) => {
                    if (error) {
                        // do nothing
                        this.showSimpleToast("Sorry, settings were not saved.");
                    }
                }).bind(this));
        };
    }

    angular.module("ngapp").controller("ControlController", ControlController);
}



//"use strict";

//angular.module("ngapp").controller("ControlController", function (shared, mobileService, $mdToast, $state, $scope, $mdSidenav, $mdComponentRegistry) {

//    var vm = this;

//    vm.relays = [
//        { title: "Pump", state: true, number: 1 },
//        { title: "Heat", state: true, number: 2 },
//        { title: "Lights", state: true, number: 3 }
//    ];

//    var showSimpleToast = function (text) {
//        $mdToast.show(
//            $mdToast.simple()
//                .textContent(text)
//                .position("top right")
//                .hideDelay(3000)
//        );
//    };


//    vm.toggle = function (relayNumber, relayState) {
//        var apiUrl = "control/setrelay?deviceId=" + shared.deviceInfo.deviceId + "&relayNumber=" + relayNumber + "&state=" + relayState;
//        mobileService.invokeApi(apiUrl, {
//            body: null,
//            method: "post"
//        }, function (error, success) {
//            if (error) {
//                // do nothing
//                showSimpleToast("Sorry, settings were not saved.");
//            }
//        });
//    };
//});
