/// <reference path="../shared/app.ts" />
interface Window {
    ripple: any;
}

namespace Submerged.Services {
    "use strict";

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

            this.emulated = window.parent && window.parent.ripple;
        }

        public init(): ng.IPromise<void> {
            var deferred = this.$q.defer<void>();

            navigator.globalization.getDatePattern((pattern) => {
                var globalizationInfoObj = new GlobalizationInfo();
                globalizationInfoObj.utc_offset = pattern.utc_offset;
                globalizationInfoObj.dst_offset = pattern.dst_offset;
                globalizationInfoObj.server_offset_seconds = pattern.utc_offset + pattern.dst_offset;

                this.globalizationInfo = globalizationInfoObj;

                console.log("Successfully loaded globalization settings.");
                this.loaded = true;

                deferred.resolve();
            }, (err) => {
                console.log("Globalization error: " + err.message);
                deferred.reject();
            });

            return deferred.promise;
        }      
    }

    angular.module("ngapp").service("sharedService", SharedService);
}