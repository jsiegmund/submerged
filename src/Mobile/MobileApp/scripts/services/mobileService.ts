/// <reference path="../shared/app.ts" />
namespace Submerged.Services {
    "use strict";

    export interface IApiCallback { (error: any, success: any): void }

    export interface IMobileService {
        init(): ng.IPromise<void>;
        login(force?: boolean): ng.IPromise<void>;
        logout(): ng.IPromise<void>;
        initPushRegistration(): void;
        unregisterPush(): void;
        invokeApi(apiName: string, options: {}, callback: IApiCallback),
        getHeaders(): ng.IPromise<any[]>;
        loggedIn(): boolean;
    }

    export class MobileService implements IMobileService {
        private mobileServiceClient: Microsoft.WindowsAzure.MobileServiceClient;
        private pushRegistration: PhonegapPluginPush.PushNotification = null;
        private registrationId: string = null;
        private loggingInPromise: ng.IPromise<void>;
        private file: string = "authtoken.json";
        private folder: string;                     // initialized during init()

        constructor(private sharedService: ISharedService, private fileService: IFileService, private $q: ng.IQService,
            private $http: ng.IHttpService, private jwtHelper: ng.jwt.IJwtHelper) {
            this.mobileServiceClient = new WindowsAzure.MobileServiceClient(this.sharedService.apiInfo.apiUrl);
            this.init();
        }

        init(): ng.IPromise<void> {
            var deferred = this.$q.defer<void>();

            console.log("Initializing mobileService for communication with Azure");
            this.folder = window.cordova.file.applicationStorageDirectory;

            console.log(`Initializing user data from storage @ ${this.folder}.`);
            this.fileService.getJsonFile<any>(this.file, this.folder).then(function (contents) {
                console.log(`File ${this.file} loaded. Contents: ${contents}`);

                if (contents != null) {
                    console.log("Found stored auth token info, applying it to service client.");
                    this.mobileServiceClient.currentUser = {
                        userId: contents.userId,
                        mobileServiceAuthenticationToken: contents.token
                    }
                }

                deferred.resolve();
            }.bind(this), function (err) {
                console.log(`Failure loading ${this.file}`);
                deferred.reject();
            });

            return deferred.promise;
        }

        saveAuthInfo(): void {
            console.log("mobileService.saveAuthInfo");
            var authInfo = {
                token: this.mobileServiceClient.currentUser.mobileServiceAuthenticationToken,
                userId: this.mobileServiceClient.currentUser.userId,
            };

            this.fileService.storeJsonFile(this.file, this.folder, authInfo).then(() => {
                console.log(`File ${this.file} stored successfully.`);
            }, () => {
                console.log(`File ${this.file} could not be saved.`);
            });
        }

        getHeaders(): ng.IPromise<any[]> {
            console.log("mobileService.getHeaders");
            var deferred = this.$q.defer();

            this.ensureLoggedIn().then(function () {
                var token = this.mobileServiceClient.currentUser.mobileServiceAuthenticationToken;
                var version = this.mobileServiceClient.version;
                var userAgent = null;

                var headers = {
                    "X-ZUMO-AUTH": token,
                    "X-ZUMO-VERSION": version
                };

                deferred.resolve(headers);
            }.bind(this), function (err) {
                deferred.reject(err);
            });

            return deferred.promise;
        }

        ensureLoggedIn(): ng.IPromise<void> {
            var deferred = this.$q.defer<void>();

            if (!this.loggedIn())
                return this.login();
            else
                deferred.resolve();

            return deferred.promise;
        }

        loggedIn(): boolean {
            console.log("mobileService.loggedIn");
            // If token doesn't exist, we aren't authenticated
            if (this.mobileServiceClient.currentUser === null) return false;

            // Verify expiry time of current token
            var token = this.mobileServiceClient.currentUser.mobileServiceAuthenticationToken;
            var decoded = this.jwtHelper.decodeToken(token);
            if (typeof decoded.exp !== 'undefined') {
                // exp is a UNIX Timestamp
                var ts = Math.round((new Date()).getTime() / 1000);
                if (ts > decoded.exp) return false;
            }

            // If there isn't an expiry or we haven't expired, we are authenticated
            return true;
        }

        login(force?: boolean): ng.IPromise<void> {
            console.log(`Performing login. Forced = ${force}`);
            var deferred = this.$q.defer<void>();

            // set the default value of the force parameter to false
            var force = typeof force !== 'undefined' ? force : false;

            // when we're already logging in; return the existing promise
            if (this.loggingInPromise != null)
                return this.loggingInPromise;

            // Login to the service
            if (!this.loggedIn() || force) {

                this.loggingInPromise = deferred.promise;

                console.log("Logging in MobileServiceClient with aad authentication.");
                this.mobileServiceClient.login('aad').then((() => {
                    this.saveAuthInfo();

                    // when successfully logged in; (re)register the device for push notifications
                    this.initPushRegistration();

                    console.log("Successful login with Azure mobile app.");
                    deferred.resolve();

                    this.loggingInPromise = null;
                }),
                    function (err) {
                        console.log("Could not login into mobile services: " + err);
                        deferred.reject(err);

                        this.loggingInPromise = null;
                    }.bind(this)
                );
            }
            else {
                console.log("Login resolving because already logged or logging in.");
                deferred.resolve();

                this.loggingInPromise = null;
            }

            return deferred.promise;
        }

        logout(): ng.IPromise<void> {
            var deferred = this.$q.defer<void>();

            this.$q.when(this.mobileServiceClient.logout().then(() => {
                // override the stored authentication info with null
                return this.fileService.storeJsonFile(this.file, this.folder, null);
            }, () => { deferred.reject(); })).then(() => {
                var logoutUrl = "https://login.windows.net/common/oauth2/logout";

                // after the mobile service clien has been logged out; do an explicit call to
                // the AAD oauth endpoint which will clear the stored cookies and session
                var browser = window.open(logoutUrl, '_blank', 'hidden=yes');

                browser.addEventListener('loadstop', (event) => {
                    browser.close();
                    deferred.resolve();
                });
            }, () => { deferred.reject(); });

            return deferred.promise;
        }

        initPushRegistration(): void {
            // when running emulated; skip push because it's not supported / working anyway
            if (this.sharedService.emulated)
                return;

            /**
            * Register for Push Notifications - requires the phonegap-plugin-push be installed
            */
            console.log("Initializing push notifications");

            this.pushRegistration = window.PushNotification.init({
                android: {
                    senderID: this.sharedService.apiInfo.gcmSenderID
                }
            });

            this.pushRegistration.on('registration', (data) => {
                this.registrationId = data.registrationId;
                this.registerPush();
            });

            this.pushRegistration.on('notification', (data) => {
                if (data && data.message && data.message.length > 0) {
                    alert(data.message);
                }
            });

            this.pushRegistration.on('error', (err) => {
                console.log('Push registration returned an error: ' + err);
            });
        }

        registerPush(): void {
            console.log("Registering device for push notifications: " + this.registrationId);
            if (this.registrationId.length > 0) {
                console.log("gcm id " + this.registrationId);
                if (this.mobileServiceClient) {
                    console.log("registering with Azure for GCM notifications");
                    this.mobileServiceClient.push.register('gcm', this.registrationId, null, null, this.registrationCallback);
                    this.updateNotificationRegistration(this.registrationId);
                }
            }
        }

        updateNotificationRegistration(registrationId: string): ng.IPromise<void> {
            var deferred = this.$q.defer<void>();
            var apiUrl = "notifications?registrationId=" + registrationId;

            this.mobileServiceClient.invokeApi(apiUrl, {
                body: null,
                method: "post"
            }, (error, success) => {
                if (error) {
                    // do nothing
                    console.log("Error calling /notifications to set notification registration: " + error);
                    deferred.reject();
                }
                else {
                    console.log("Successfully updated device registration properties via the back-end.");
                    deferred.resolve();
                }
            });

            return deferred.promise;

        }


        unregisterPush(): void {
            console.log("unregistering for push messages");
            this.mobileServiceClient.push.unregister(this.registrationId, this.unregisterCallback);
        }

        unregisterCallback(error, success): void {

        }

        registrationCallback(error, success): void {
            console.log("registrationCallback called.");
        }

        invokeApi(apiName: string, options: Microsoft.WindowsAzure.InvokeApiOptions, callback: IApiCallback): void {
            this.ensureLoggedIn().then(() => {
                this.mobileServiceClient.invokeApi(apiName, options, callback);
            }, function () {
                console.log("Cannot invoke API because login failed.");
            });
        }
    }

    angular.module("ngapp").service('mobileService', MobileService);
}