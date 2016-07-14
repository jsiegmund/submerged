namespace Submerged.Controllers {
    export class ControlController {

        relays: Models.RelayModel[];
        deviceId: string;
        maintenanceMode: boolean;
        disabled: boolean = true;

        constructor(private sharedService: Services.ISharedService, private mobileService: Services.IMobileService, private $mdToast: ng.material.IToastService,
            private $state: ng.ui.IStateService) {

            this.deviceId = sharedService.settings.getDeviceId();
            this.maintenanceMode = sharedService.settings.getDevice().deviceProperties.isInMaintenance;
            this.loadRelays();
        }

        loadRelays() {
            var apiUrl = "control/relays?deviceId=" + this.deviceId;

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, ((error, success) => {
                if (error) {
                    this.showSimpleToast("Sorry, settings were not saved.");
                }
                else {
                    this.processRelays(success.result);
                    this.disabled = false;
                }
            }).bind(this));
        }

        toggleMaintenance() {
            this.disabled = true;
            this.sharedService.settings.getDevice().deviceProperties.isInMaintenance = this.maintenanceMode;
            var apiUrl = "control/maintenance/toggle?deviceId=" + this.deviceId + "&inMaintenance=" + this.maintenanceMode;

            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, ((error, success) => {
                if (error) {
                    this.showSimpleToast("Sorry, settings were not saved.");
                }
                else {
                    this.loadRelays();
                    this.showSimpleToast("Maintenance mode toggled!");
                }
            }).bind(this));
        }

        processRelays(relays: Models.RelayModel[]) {
            this.relays = relays;
            this.disabled = false;
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
            this.disabled = true;
            var apiUrl = "control/setrelay?deviceId=" + this.deviceId + "&relayNumber=" + relayNumber + "&state=" + relayState;
            this.mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, ((error, success) => {
                    if (error) {
                        this.showSimpleToast("Sorry, settings were not saved.");
                    }
                    else {
                        this.showSimpleToast("Toggle command sent!");
                        this.disabled = false;
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
