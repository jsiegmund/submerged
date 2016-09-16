namespace Submerged.Controllers {

    interface ISettingsModuleScope extends ng.IScope {
    }

    export class SettingsModuleController extends BaseController {

        module: Models.ModuleModel;
        moduleTypeOptions: any[];

        constructor(private subscriptionService: Services.ISubscriptionService, private $scope: ISettingsModuleScope,
            private $location: ng.ILocationService, $mdToast: ng.material.IToastService, menuService: Services.IMenuService,
            private $compile: ng.ICompileService) {

            super($mdToast);

            this.moduleTypeOptions = subscriptionService.getModuleTypes();
            this.module = subscriptionService.getSelectedModule();

            this.injectConfigSection();

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

        injectConfigSection() {
            if (this.module &&
                this.module.moduleType == Statics.MODULETYPES.LEDENET) {
                var el = this.$compile("<sm-module-ledenet-configuration />")(this.$scope);
                el.attr("data-module", "{vm.module}");
                angular.element(document.querySelector('#sm-module-config-container')).append(el);
            }
        }

        showConfigSection(): boolean {
            if (this.module && this.module.moduleType == Statics.MODULETYPES.LEDENET)
                return true;
            else
                return false;
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

    angular.module("ngapp").controller("SettingsModuleController", SettingsModuleController);
}