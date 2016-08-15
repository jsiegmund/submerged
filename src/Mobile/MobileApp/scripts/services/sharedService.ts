interface Window {
    ripple: any;
}

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

    export interface ISharedService {
        emulated: boolean;
        apiInfo: ApiInfo;
        globalizationInfo: GlobalizationInfo;

        init(): ng.IPromise<void>;
    }

    export class SharedService implements ISharedService {

        apiInfo: ApiInfo;
        globalizationInfo: GlobalizationInfo;
        emulated: boolean; 

        loaded: boolean = false;

        constructor(private $q: ng.IQService) {
            console.log("Constructor of shared called");

            var apiInfoObj = new ApiInfo();
            apiInfoObj.apiUrl = 'https://neptune-mobileapi.azurewebsites.net';
            apiInfoObj.baseUrl = apiInfoObj.apiUrl + '/api';
            apiInfoObj.signalRUrl = apiInfoObj.apiUrl;
            apiInfoObj.gcmSenderID = '532189147734';
            this.apiInfo = apiInfoObj;

            var globalizationInfoObj = new GlobalizationInfo();
            this.globalizationInfo = globalizationInfoObj;

            this.emulated = window.parent && window.parent.ripple;
        }

        public init(): ng.IPromise<void> {
            var deferred = this.$q.defer<void>();

            navigator.globalization.getDatePattern((pattern) => {
                this.globalizationInfo.utc_offset = pattern.utc_offset;
                this.globalizationInfo.dst_offset = pattern.dst_offset;
                this.globalizationInfo.server_offset_seconds = pattern.utc_offset + pattern.dst_offset;

                console.log("Successfully loaded globalization settings.");
                this.loaded = true;

                deferred.resolve();
            }, (err) => {
                console.log("Globalization error: " + err.message);
                deferred.reject();
            });

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

    angular.module("ngapp").service("sharedService", SharedService);
}