"use strict";

namespace Submerged.Services {

    export class ApiInfo {
        apiUrl: string;
        baseUrl: string;
        signalRUrl: string;
        gcmSenderID: string;
    }

    export class DeviceInfo {
        deviceId: string;
    }

    export class GlobalizationInfo {
        utc_offset: number;
        dst_offset: number;
    }

    export interface ISettings {
        modules: Models.ModuleModel[];
        sensors: Models.SensorModel[];
    }

    export class Settings implements ISettings {
        modules: Models.ModuleModel[];
        sensors: Models.SensorModel[];

        constructor() {
        }
    }

    export interface IShared {
        apiInfo: ApiInfo;
        deviceInfo: DeviceInfo;
        globalizationInfo: GlobalizationInfo;
        settings: ISettings;

        init(): void;
        save(): void;
        loadSettings(mobileService: IMobileService): ng.IPromise<ISettings>;
        loadSettingsFromCloud(mobileService: IMobileService): ng.IPromise<ISettings>;
    }

    export class Shared implements IShared {

        settings: ISettings;
        apiInfo: ApiInfo;
        deviceInfo: DeviceInfo;
        globalizationInfo: GlobalizationInfo;
        file: string = "settings.json";

        static $inject = ['fileService', '$q' ];

        constructor(private fileService: IFileService, private $q: ng.IQService) {
            console.log("Constructor of shared called");

            var apiInfoObj = new ApiInfo();
            apiInfoObj.apiUrl = 'https://neptune-mobileapi.azurewebsites.net';
            apiInfoObj.baseUrl = "https://neptune-mobileapi.azurewebsites.net/api";
            apiInfoObj.signalRUrl = "https://neptune-mobileapi.azurewebsites.net/";
            apiInfoObj.gcmSenderID = '532189147734';
            this.apiInfo = apiInfoObj;

            var deviceInfoObj = new DeviceInfo();
            deviceInfoObj.deviceId = "repsaj-neptune-win10pi";
            this.deviceInfo = deviceInfoObj;

            var globalizationInfoObj = new GlobalizationInfo();
            globalizationInfoObj.dst_offset = 0;
            globalizationInfoObj.utc_offset = 0;
            this.globalizationInfo = globalizationInfoObj;
        }

        public init(): void {

        }

        public save(): void {
            var folder = window.cordova.file.applicationStorageDirectory;
            this.fileService.storeJsonFile(this.file, folder, this.settings);
        }

        private setSettings(settings: ISettings, save: boolean) {
            console.log("setSettings");
            this.settings = new Settings();
            angular.copy(settings, this.settings);

            if (save)
                this.save();
        }

        public loadSettingsFromCloud(mobileService: IMobileService): ng.IPromise<ISettings> {
            console.log("loadSettingsFromCloud");

            var deferred = this.$q.defer<ISettings>();

            this.loadFromCloud(mobileService).then(((settings: ISettings) => {
                this.setSettings(settings, true);
                deferred.resolve(settings);
            }))

            return deferred.promise;
        }

        public loadSettings(mobileService: IMobileService): ng.IPromise<ISettings> {
            return this.loadFromFile().then(((settings: ISettings) => {
                this.setSettings(settings, false);
                return settings;
            }), ((reason: any) => {
                    return this.loadSettingsFromCloud(mobileService);
            }));
        }

        private loadFromFile(): ng.IPromise<ISettings> {
            var deferred = this.$q.defer<ISettings>();
            var folder = window.cordova.file.applicationStorageDirectory;

            console.log("Initializing settings from storage.");            
            this.fileService.getJsonFile<Settings>(this.file, folder).then(
                function (storedSettings) {
                    if (storedSettings != null) {
                         deferred.resolve(storedSettings);
                    } else {
                        deferred.reject();
                    }
                },
                function () {
                    deferred.reject();
                }
            );

            return deferred.promise;
        }

        private loadFromCloud(mobileService: IMobileService): ng.IPromise<ISettings> {
            var deferred = this.$q.defer<ISettings>();

            this.$q.all([
                this.loadSensors(this.deviceInfo.deviceId, mobileService),
                this.loadModules(this.deviceInfo.deviceId, mobileService)
            ]).then(function(values) {

                var settings: ISettings = new Settings();
                settings.sensors = values[0];
                settings.modules = values[1];

                deferred.resolve(settings);

            }, function (err) {
                deferred.reject();
            });

            return deferred.promise;
        }

        loadSensors(deviceId: string, mobileService: IMobileService): ng.IPromise<Models.SensorModel[]> {
            var deferred = this.$q.defer<Models.SensorModel[]>();
            var apiUrl = "sensors?deviceId=" + deviceId;
            mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, ((error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /sensors to load the sensor settings: " + error);
                    deferred.reject();
                }
                else {
                    deferred.resolve(success.result);
                }
            }));

            return deferred.promise;
        }

        loadModules(deviceId: string, mobileService: IMobileService): ng.IPromise<Models.ModuleModel[]> {
            var deferred = this.$q.defer<Models.ModuleModel[]>();
            var apiUrl = "sensors?deviceId=" + deviceId;
            mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, ((error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /sensors to load the sensor settings: " + error);
                    deferred.reject();
                }
                else {
                    deferred.resolve(success.result);
                }
            }));

            return deferred.promise;
        }


    }

    angular.module("ngapp").service("shared", Shared);
}