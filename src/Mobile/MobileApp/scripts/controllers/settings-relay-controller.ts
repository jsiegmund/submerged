/// <reference path="../shared/app.ts" />
namespace Submerged.Controllers {
    "use strict";

    export class SettingsRelayController extends BaseController {

        relay: Models.RelayModel;
        pinConfigOptions: string[];
        moduleOptions: Models.ModuleModel[];

        constructor(private subscriptionService: Services.ISubscriptionService, private $scope: ng.IRootScopeService,
            private $location: ng.ILocationService, $mdToast: ng.material.IToastService, menuService: Services.IMenuService) {

            super($mdToast);

            this.pinConfigOptions = subscriptionService.getPinList();
            this.moduleOptions = subscriptionService.getSelectedDevice().modules; 

            this.relay = subscriptionService.getSelectedRelay();
            if (this.relay.pinConfig == undefined || this.relay.pinConfig.length == 0)
                this.relay.pinConfig = [''];

            // add delete button to remove existing sensor
            if (this.relay.name != undefined) {
                var deleteButton = new Services.CommandButton();
                deleteButton.svgSrc = 'icons/ic_delete_24px.svg';
                deleteButton.clickHandler = this.delete;
                deleteButton.label = 'Delete';
                deleteButton.owner = this;

                menuService.setButtons([deleteButton]);
            }
        }

        delete() {
            this.busy = true;

            this.subscriptionService.deleteRelay(this.relay).then(() => {
                window.history.back();      // relay deleted; go back
                this.busy = false;
            }, () => {
                this.showSimpleToast("Sorry, settings were not saved.");
                this.busy = false;
            });
        }

        save() {
            this.busy = true;

            this.subscriptionService.saveRelay(this.relay).then(() => {
                this.showSimpleToast("Relay settings saved!");
                this.busy = false;
            }, () => {
                this.showSimpleToast("Sorry, settings were not saved.");
                this.busy = false;
            });
        }

    }

    angular.module("ngapp").controller("SettingsRelayController", SettingsRelayController);
}