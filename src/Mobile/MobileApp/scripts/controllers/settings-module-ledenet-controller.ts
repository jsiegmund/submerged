/// <reference path="../shared/app.ts" />
namespace Submerged.Controllers {
    "use strict";

    interface ISettingsModuleScope extends ng.IScope {
        colorPickerOptions: any;
    }

    export class SettingsModuleLedenetController extends BaseController {

        point: Models.ModuleConfiguration.LedenetPointInTimeDisplayModel;
        config: Models.ModuleConfiguration.LedenetModuleConfigurationModel;
        module: Models.ModuleModel;

        minDate: number;
        maxDate: number;

        constructor(private subscriptionService: Services.ISubscriptionService, private $scope: ISettingsModuleScope,
            private $location: ng.ILocationService, $mdToast: ng.material.IToastService, menuService: Services.IMenuService,
            private $compile: ng.ICompileService, private ledenetModuleService: Services.ILedenetModuleService,
            private sharedService: Services.ISharedService) {

            super($mdToast);

            var today = new Date(moment.now());
            var tomorrow = new Date(new Date(moment.now()).setDate(today.getDate() + 1));

            this.minDate = new Date(today.getFullYear(), today.getMonth(), today.getDate(), 0, 0, 0).valueOf();
            this.maxDate = new Date(tomorrow.getFullYear(), tomorrow.getMonth(), tomorrow.getDate(), 0, 0, 0).valueOf();

            this.module = subscriptionService.getSelectedModule();
            this.config = this.module.configuration;
            this.point = <Models.ModuleConfiguration.LedenetPointInTimeDisplayModel>ledenetModuleService.getSelectedPointInTime();

            if (this.point.time) {
                // initialize the string representation of the time
                this.point.timeString = this.timeAsString(this.point.time);
            }

            $scope.$watch(() => { return this.point.timeString; }, (newValue, oldValue) => {
                if (newValue) {
                    // we need to conver the timeString to a UTC representation of the minutes
                    var timeMoment = moment(this.point.timeString, "HH:mm");
                    timeMoment = timeMoment.add("seconds", this.sharedService.globalizationInfo.server_offset_seconds);
                    this.point.time = (timeMoment.hours() * 60) + timeMoment.minutes();
                }
            });

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

        timeAsString(time: number) {
            var utcOffsetSeconds = this.sharedService.globalizationInfo.server_offset_seconds;
            var localTime = time + (utcOffsetSeconds / 60);
            var hours = Math.floor(localTime / 60);
            var minutes = localTime % 60;
            var now = new Date(Date.now());
            var date = new Date(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDay(), hours, minutes, 0);
            return moment(date).format("HH:mm");
        }

        pointClicked(point: Models.ModuleConfiguration.LedenetPointInTime) {
            this.ledenetModuleService.selectPointInTime(point);
            this.$location.path("/settings/module/ledenet/pointintime");
        }

        newPointInTime() {
            this.ledenetModuleService.selectPointInTime(null);
            this.$location.path("/settings/module/ledenet/pointintime");
        }

        testProgram() {
            this.ledenetModuleService.testProgram();
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