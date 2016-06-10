namespace Submerged.Controllers {

    export class SettingsController {
        loading: boolean = true;
        saving: boolean = false;
        sensors: any[];
        indexedRules = [];

        constructor(private shared: Services.IShared, private mobileService: Services.IMobileService, private fileService: Services.IFileService,
            private $scope: ng.IRootScopeService, private $location: ng.ILocationService, private $mdToast: ng.material.IToastService) {

            var apiUrl = "sensors?deviceId=" + shared.deviceInfo.deviceId;
            mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, ((error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /data/sensors to the sensor configuration: " + error);
                }
                else {
                    this.processData(success.result);      // process the last known data for display
                }
            }).bind(this));
        }

        processData(sensors: Models.SensorRuleModel[]): void {

            for (var sensor of sensors) {
                switch (sensor.sensorType) {
                    case "temperature":
                        sensor.minimumValue = 10;
                        sensor.maximumValue = 40;
                        sensor.step = 1;
                        break;
                    case "pH":
                        sensor.minimumValue = 5.5;
                        sensor.maximumValue = 8;
                        sensor.step = 0.1;
                        break;
                }
            }

            this.sensors = sensors;
            this.loading = false;
            this.$scope.$apply();
        }

        showSimpleToast(text: string): void {
            this.$mdToast.show(
                this.$mdToast.simple()
                    .textContent(text)
                    .position("top right")
                    .hideDelay(3000)
            );
        }

        save(): void {
            this.saving = true;

            this.shared.settings.sensors = this.sensors;
            this.shared.save();

            var apiUrl = "sensors/save?deviceId=" + this.shared.deviceInfo.deviceId;
            this.mobileService.invokeApi(apiUrl, {
                body: this.shared.settings.sensors,
                method: "post"
            }, ((error, success) => {
                this.saving = false;

                if (error) {
                    // do nothing
                    this.showSimpleToast("Sorry, settings were not saved.");
                }
            }).bind(this));
        }
    }

    angular.module("ngapp").controller("SettingsController", SettingsController);
}

//"use strict";

//angular.module("ngapp").controller("SettingsController", function (shared, mobileService, fileService, $scope, $location, $mdToast) {
//    var vm = this;

//    vm.loading = true;
//    vm.saving = false;

//    function processData(data) {
//        vm.rules = data;
//        vm.loading = false;
//        $scope.$apply();
//    }

//    var indexedRules = [];

//    vm.rulesToFilter = function () {
//        indexedRules = [];
//        return vm.rules;
//    }

//    vm.groupRules = function (rule) {
//        var ruleIsNew = indexedRules.indexOf(rule.name) == -1;
//        if (ruleIsNew) {
//            indexedRules.push(rule.name);
//        }
//        return ruleIsNew;
//    }

//    var showSimpleToast = function (text) {
//        $mdToast.show(
//            $mdToast.simple()
//                .textContent(text)
//                .position("top right")
//                .hideDelay(3000)
//        );
//    };

//    vm.save = function () {
//        vm.saving = true;

//        var settings = shared.settings;
//        settings.rules = vm.rules;
//        settings.save();

//        var apiUrl = "rules/save?deviceId=" + shared.deviceInfo.deviceId;
//        mobileService.invokeApi(apiUrl, {
//            body: settings.rules,
//            method: "post"
//        }, function (error, success) {
//            vm.saving = false;

//            if (error) {
//                // do nothing
//                showSimpleToast("Sorry, settings were not saved.");
//            }
//        });
//    }

//    function init() {

//        var apiUrl = "rules?deviceId=" + shared.deviceInfo.deviceId;
//        mobileService.invokeApi(apiUrl, {
//            body: null,
//            method: "post"
//        }, function (error, success) {
//            if (error) {
//                // do nothing
//                console.log("Error calling /rules to get device rules: " + error);
//            }
//            else {
//                processData(success.result);      // process the last known data for display
//            }
//        });
//    }

//    init();
//});
