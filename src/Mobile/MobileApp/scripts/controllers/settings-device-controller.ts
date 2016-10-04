﻿namespace Submerged.Controllers {
    export class SettingsDeviceController extends BaseController {

        device: Models.DeviceModel;

        constructor(private subscriptionService: Services.ISubscriptionService, private $scope: ng.IRootScopeService,
            private $location: ng.ILocationService, $mdToast: ng.material.IToastService, menuService: Services.IMenuService) {

            super($mdToast);

            this.device = this.subscriptionService.getSelectedDevice();
        }

        delete() {
            this.busy = true;

            this.subscriptionService.deleteDevice(this.device).then(() => {
                window.history.back();      // device deleted; go back
                this.busy = false;
            }, () => {
                this.showSimpleToast("Sorry, settings were not saved.");
                this.busy = false;
            });
        }

        save() {
            this.busy = true;

            this.subscriptionService.saveDevice(this.device).then(() => {
                this.showSimpleToast("Device settings saved!");
            }, (error) => {
                console.log(`Failure saving device settings: ${error}`);
                this.showSimpleToast("Sorry, settings were not saved.");
            }).finally(() => {
                this.busy = false;
            });
        }

    }

    angular.module("ngapp").controller("SettingsDeviceController", SettingsDeviceController);
}