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
        server_offset_seconds: number;

        constructor() {
            this.dst_offset = 0;
            this.utc_offset = 0;
            this.server_offset_seconds = 0;
        }
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

        save(): void;
        init(mobileService: IMobileService): ng.IPromise<{}>;
        loadSettings(mobileService: IMobileService): ng.IPromise<ISettings>;
        loadSubscriptionFromCloud(mobileService: IMobileService): ng.IPromise<ISettings>;
    }

    export class Shared implements IShared {

        settings: ISettings;
        file: string = "subscription.json";

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
            this.settings.globalizationInfo = globalizationInfoObj;
        }

        public init(mobileService: IMobileService): ng.IPromise<{}> {
            var globalizationPromise = this.loadGlobalizationSettings();
            var settingsPromise = this.loadSubscriptionFromCloud(mobileService);

            var deferred = this.$q.defer<ISettings>();

            // wait before everything has loaded before returning the actual settings object
            this.$q.all([globalizationPromise, settingsPromise]).then(function (result) {
                deferred.resolve(result.values[1]);
            }, (error) => {
                console.log("Failed initializing shared settings: " + error);
                deferred.reject(error);
            });

            return deferred.promise;
        }

        public save(): void {
            var folder = window.cordova.file.applicationStorageDirectory;
            this.fileService.storeJsonFile(this.file, folder, this.settings.subscription);
        }

        private setSubscription(subscription: Models.SubscriptionModel, save: boolean) {
            console.log("setSettings");

            this.settings.subscription = new Models.SubscriptionModel();
            angular.copy(subscription, this.settings.subscription);

            if (save)
                this.save();
        }

        private loadGlobalizationSettings(): ng.IPromise<void> {
            var deferred = this.$q.defer<void>();

            navigator.globalization.getDatePattern((pattern) => {
                console.log(`setting globalization info to utc_offset: ${pattern.utc_offset}.`);
                this.settings.globalizationInfo.utc_offset = pattern.utc_offset;
                this.settings.globalizationInfo.dst_offset = pattern.dst_offset;
                this.settings.globalizationInfo.server_offset_seconds = pattern.utc_offset + pattern.dst_offset;
                deferred.resolve();
            }, (err) => {
                console.log("Globalization error: " + err.message);
                deferred.reject();
            });

            return deferred.promise;
        }

        public loadSubscriptionFromCloud(mobileService: IMobileService): ng.IPromise<ISettings> {
            console.log("loadSubscriptionFromCloud");

            var deferred = this.$q.defer<ISettings>();

            this.loadFromCloud(mobileService).then((subscription: Models.SubscriptionModel) => {
                this.setSubscription(subscription, true);
                deferred.resolve(this.settings);
            });

            return deferred.promise;
        }

        public loadSettings(mobileService: IMobileService): ng.IPromise<ISettings> {
            return this.loadFromFile().then(((subscription: Models.SubscriptionModel) => {
                this.setSubscription(subscription, false);
                return this.settings;
            }), ((reason: any) => {
                return this.loadSubscriptionFromCloud(mobileService);
            }));
        }

        private loadFromFile(): ng.IPromise<Models.SubscriptionModel> {
            var deferred = this.$q.defer<Models.SubscriptionModel>();
            var folder = window.cordova.file.applicationStorageDirectory;

            console.log("Initializing settings from storage.");            
            this.fileService.getJsonFile<Models.SubscriptionModel>(this.file, folder).then(
                function (subscription) {
                    if (subscription != null) {
                        deferred.resolve(subscription);
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

        private loadFromCloud(mobileService: IMobileService): ng.IPromise<Models.SubscriptionModel> {
            var deferred = this.$q.defer<Models.SubscriptionModel>();

            this.loadSubscription(mobileService).then(function(subscription) {
                // build a new settings object and save the subscription in it
                deferred.resolve(subscription);

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