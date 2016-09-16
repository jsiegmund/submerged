namespace Submerged.Controllers {

    interface ISettingsModuleScope extends ng.IScope {
        colorPickerOptions: any;
    }

    export class SettingsModuleLedenetController extends BaseController {

        point: Models.ModuleConfiguration.LedenetPointInTime;
        config: Models.ModuleConfiguration.LedenetModuleConfigurationModel;
        module: Models.ModuleModel;

        constructor(private subscriptionService: Services.ISubscriptionService, private $scope: ISettingsModuleScope,
            private $location: ng.ILocationService, $mdToast: ng.material.IToastService, menuService: Services.IMenuService,
            private $compile: ng.ICompileService, private ledenetModuleService: Services.ILedenetModuleService) {

            super($mdToast);

            this.module = subscriptionService.getSelectedModule();
            this.config = this.module.configuration;
            this.point = ledenetModuleService.getSelectedPointInTime();  

            /* TODO: options are not working... new version of control is due, should upgrade and check again */
            $scope.colorPickerOptions = {
                label: "Pick a color",
                hex: false,
                type: "1",
                default: "rgb(255,0,0)",
                hsl: false,
                    mdColorType: "1",
                    mdColorDefault: "rgb(255,0,0)",
                    mdColorHistory: false,
                    mdColorAlphaChannel: false,
                    mdColorSpectrum: false,
                    mdColorSliders: false,
                    mdColorGenericPalette: false,
                    mdColorMaterialPalette: false,
                    mdColorHex: false,
                    mdColorHsl: false
            };

            // add delete button to remove existing sensor
            if (this.module.name != undefined) {
                var deleteButton = new Services.CommandButton();
                deleteButton.svgSrc = 'icons/ic_delete_24px.svg';
                deleteButton.clickHandler = this.delete;
                deleteButton.label = 'Delete';
                deleteButton.owner = this;

                menuService.setButtons([deleteButton]);
            }
        }

        pointClicked(point: Models.ModuleConfiguration.LedenetPointInTime) {
            this.ledenetModuleService.selectPointInTime(point);
            this.$location.path("/settings/module/ledenet/pointintime");
        }

        newPointInTime() {
            this.ledenetModuleService.selectPointInTime(null);
            this.$location.path("/settings/module/ledenet/pointintime");
        }

        savePoint() {
            this.ledenetModuleService.savePoint(this.point);
            this.$location.path("settings/module");
        }

        delete() {
            this.busy = true;

            this.subscriptionService.deleteModule(this.module).then(() => {
                window.history.back();      // module deleted; go back
                this.busy = false;
            }, () => {
                this.showSimpleToast("Sorry, settings were not saved.");
                this.busy = false;
            });
        }

        save() {
            this.busy = true;

            this.subscriptionService.saveModule(this.module).then(() => {
                this.showSimpleToast("Module settings saved!");
                this.busy = false;
            }, () => {
                this.showSimpleToast("Sorry, settings were not saved.");
                this.busy = false;
            });
        }
    }

    angular.module("ngapp").controller("SettingsModuleLedenetController", SettingsModuleLedenetController);
}