namespace Submerged.Controllers {
    export class ControlController {

        relays: Models.RelayModel[];
        deviceId: string;
        maintenanceMode: boolean;
        disabled: boolean = true;

        constructor(private sharedService: Services.ISharedService, private mobileService: Services.IMobileService, private $mdToast: ng.material.IToastService,
            private $state: ng.ui.IStateService, private dataService: Services.IDataService, private subscriptionService: Services.SubscriptionService) {

            this.deviceId = subscriptionService.getSelectedDeviceID();
            this.maintenanceMode = subscriptionService.getSelectedDevice().deviceProperties.isInMaintenance;

            this.loadRelays();
        }

        loadRelays() {
            this.dataService.getRelays(this.deviceId).then(
                this.processRelays,
                (error) => { this.showSimpleToast("Could not load relays"); }
            );
        }

        toggleMaintenance() {
            this.disabled = true;
            this.subscriptionService.getSelectedDevice().deviceProperties.isInMaintenance = this.maintenanceMode;

            this.dataService.toggleMaintenance(this.deviceId, this.maintenanceMode).then(
                () => {
                    this.showSimpleToast("Sorry, settings were not saved.");
                },
                () => {
                    this.loadRelays();
                    this.showSimpleToast("Maintenance mode toggled!");
                }
            );
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

        toggle(relayName: string, relayState: boolean) {
            this.disabled = true;

            this.dataService.toggleRelay(this.deviceId, relayName, relayState).then(
                () => {
                    this.showSimpleToast("Toggle command sent!");
                    this.disabled = false;
                },
                () => {
                    this.showSimpleToast("Sorry, settings were not saved.");
                    this.disabled = false;
                }
            );
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
