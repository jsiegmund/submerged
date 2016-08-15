namespace Submerged.Controllers {
    export class SettingsTankController extends BaseController {

        tank: Models.TankModel;

        constructor(private subscriptionService: Services.ISubscriptionService, private $scope: ng.IRootScopeService,
            private $location: ng.ILocationService, $mdToast: ng.material.IToastService, menuService: Services.IMenuService) {

            super($mdToast);

            this.tank = subscriptionService.getSelectedTank();

            // add delete button to remove existing tank
            if (this.tank.name != undefined) {
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

            this.subscriptionService.deleteTank(this.tank).then(() => {
                window.history.back();      // tank deleted; go back
                this.busy = false;
            }, () => {
                this.showSimpleToast("Sorry, settings were not saved.");
                this.busy = false;
            });
        }

        save() {
            this.busy = true;

            this.subscriptionService.saveTank(this.tank).then(() => {
                this.showSimpleToast("Tank settings saved!");
                this.busy = false;
            }, () => {
                this.showSimpleToast("Sorry, settings were not saved.");
                this.busy = false;
            });
        }

    }

    angular.module("ngapp").controller("SettingsTankController", SettingsTankController);
}