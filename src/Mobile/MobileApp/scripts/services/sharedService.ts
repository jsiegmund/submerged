"use strict";

namespace Submerged.Services {

    export class ApiInfo {
        apiUrl: string;
        baseUrl: string;
        signalRUrl: string;
        gcmSenderID: string;
    }

    export class GlobalizationInfo {
        utc_offset: number;
        dst_offset: number;
    }

    export interface ISettings {
        subscription: Models.SubscriptionModel;
        apiInfo: ApiInfo;
        globalizationInfo: GlobalizationInfo;
        getDeviceId(): string;
    }

    export class Settings implements ISettings {
        subscription: Models.SubscriptionModel;
        apiInfo: ApiInfo;
        globalizationInfo: GlobalizationInfo;

        constructor() {
        }

        getDeviceId(): string {
            if (this.subscription != null &&
                this.subscription.devices != null &&
                this.subscription.devices.length > 0)
                return this.subscription.devices[0].deviceProperties.deviceID;
            else
                return "";
        }
    }

    export interface IShared {
        settings: ISettings;

        init(): void;
        save(): void;
        loadSettings(mobileService: IMobileService): ng.IPromise<ISettings>;
        loadSettingsFromCloud(mobileService: IMobileService): ng.IPromise<ISettings>;
    }

    export class Shared implements IShared {

        settings: ISettings;
        apiInfo: ApiInfo;
        globalizationInfo: GlobalizationInfo;
        file: string = "settings.json";

        static $inject = ['fileService', '$q' ];

        constructor(private fileService: IFileService, private $q: ng.IQService) {
            console.log("Constructor of shared called");

            this.settings = new Settings();

            var apiInfoObj = new ApiInfo();
            apiInfoObj.apiUrl = 'https://neptune-mobileapi.azurewebsites.net';
            apiInfoObj.baseUrl = apiInfoObj.apiUrl + '/api';
            apiInfoObj.signalRUrl = apiInfoObj.apiUrl;
            apiInfoObj.gcmSenderID = '532189147734';
            this.settings.apiInfo = apiInfoObj;

            var globalizationInfoObj = new GlobalizationInfo();
            globalizationInfoObj.dst_offset = 0;
            globalizationInfoObj.utc_offset = 0;
            this.settings.globalizationInfo = globalizationInfoObj;

            navigator.globalization.getDatePattern(this.globalizationSuccess, this.globalizationError);
        }

        globalizationSuccess(pattern) {
            this.globalizationInfo.utc_offset = pattern.utc_offset;
            this.globalizationInfo.dst_offset = pattern.dst_offset;
        }

        globalizationError(globalizationError) {
            console.log("Globalization error: " + globalizationError.message);
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

            this.loadSubscription(mobileService).then(function(subscription) {

                // build a new settings object and save the subscription in it
                var settings: ISettings = new Settings();
                settings.subscription = subscription;

                deferred.resolve(settings);

            }, function (err) {
                deferred.reject();
            });

            return deferred.promise;
        }

        loadSubscription(mobileService: IMobileService): ng.IPromise<Models.SubscriptionModel> {
            var deferred = this.$q.defer<Models.SubscriptionModel>();
            var apiUrl = "subscription";
            mobileService.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, ((error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /subscription to load the subscription details: " + error);
                    deferred.reject();
                }
                else {
                    deferred.resolve(success.result);
                }
            }));

            return deferred.promise;
        }

        //loadSensors(deviceId: string, mobileService: IMobileService): ng.IPromise<Models.SensorModel[]> {
        //    var deferred = this.$q.defer<Models.SensorModel[]>();
        //    var apiUrl = "sensors?deviceId=" + deviceId;
        //    mobileService.invokeApi(apiUrl, {
        //        body: null,
        //        method: "post"
        //    }, ((error, success) => {
        //        if (error) {
        //            // do nothing
        //            console.log("Error calling /sensors to load the sensor settings: " + error);
        //            deferred.reject();
        //        }
        //        else {
        //            deferred.resolve(success.result);
        //        }
        //    }));

        //    return deferred.promise;
        //}

        //loadModules(deviceId: string, mobileService: IMobileService): ng.IPromise<Models.ModuleModel[]> {
        //    var deferred = this.$q.defer<Models.ModuleModel[]>();
        //    var apiUrl = "sensors?deviceId=" + deviceId;
        //    mobileService.invokeApi(apiUrl, {
        //        body: null,
        //        method: "post"
        //    }, ((error, success) => {
        //        if (error) {
        //            // do nothing
        //            console.log("Error calling /sensors to load the sensor settings: " + error);
        //            deferred.reject();
        //        }
        //        else {
        //            deferred.resolve(success.result);
        //        }
        //    }));

        //    return deferred.promise;
        //}


    }

    angular.module("ngapp").service("shared", Shared);
}