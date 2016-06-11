"use strict";
angular.module("ngapp", ["ui.router", "ngMdIcons", "ngMaterial", "ngCordova", "ngCordova.plugins", "ngStorage", "TimerApp", "angular-jwt"])
    .run(function ($rootScope, $cordovaStatusbar, $location, shared) {
    document.addEventListener("deviceready", onDeviceReady, false);
    function onDeviceReady() {
        hockeyapp.start(null, null, "c6076e8da2b24860aed146c667f32fa9");
        $cordovaStatusbar.overlaysWebView(false); // Always Show Status Bar
        window.plugins.orientationLock.lock("portrait");
        //document.addEventListener("backbutton", onBackButton, false);
        document.addEventListener("resume", onResume, false);
        navigator.globalization.getDatePattern(globalizationSuccess, globalizationError);
    }
    function globalizationSuccess(pattern) {
        shared.globalizationInfo.utc_offset = pattern.utc_offset;
        shared.globalizationInfo.dst_offset = pattern.dst_offset;
    }
    function globalizationError(globalizationError) {
        console.log("Globalization error: " + globalizationError.message);
    }
    /* Hijack Android Back Button (You Can Set Different Functions for Each View by Checking the $state.current) */
    function onBackButton() {
        if ($location.path() == '/main') {
        }
        else {
        }
    }
    function onResume() {
        $rootScope.$broadcast('resume', null);
    }
})
    .config(function ($mdThemingProvider) {
    $mdThemingProvider.theme('altTheme')
        .primaryPalette('purple');
});
Array.prototype.select = function (expr) {
    var arr = this;
    switch (typeof expr) {
        case 'function':
            return jQuery.map(arr, expr);
        case 'string':
            try {
                var func = new Function(expr.split('.')[0], 'return ' + expr + ';');
                return jQuery.map(arr, func);
            }
            catch (e) {
                return null;
            }
        default:
            throw new ReferenceError('expr not defined or not supported');
    }
};
Array.prototype.where = function (filter) {
    var collection = this;
    switch (typeof filter) {
        case 'function':
            return jQuery.grep(collection, filter);
        case 'object':
            for (var property in filter) {
                if (!filter.hasOwnProperty(property))
                    continue; // ignore inherited properties
                collection = jQuery.grep(collection, function (item) {
                    return item[property] === filter[property];
                });
            }
            return collection.slice(0); // copy the array 
        // (in case of empty object filter)
        default:
            throw new TypeError('func must be either a' +
                'function or an object of properties and values to filter by');
    }
};
Array.prototype.firstOrDefault = function (func) {
    return this.where(func)[0] || null;
};
"use strict";
angular.module("ngapp").config(["$stateProvider", "$urlRouterProvider", function ($stateProvider, $urlRouterProvider) {
        $urlRouterProvider.otherwise("/login");
        $stateProvider
            .state("login", {
            url: "/login",
            templateUrl: "app/views/login.html",
            title: "Submerged Login",
            controller: "LoginContoller",
            controllerAs: "vm"
        })
            .state("main", {
            url: "/main",
            templateUrl: "app/views/main.html",
            title: "Submerged Mobile",
            controller: "MainController",
            controllerAs: "vm"
        })
            .state("live", {
            url: "/live",
            templateUrl: "app/views/live.html",
            title: "Submerged Live",
            controller: "LiveController",
            controllerAs: "vm",
        })
            .state("analytics", {
            url: "/anaytics",
            templateUrl: "app/views/analytics.html",
            title: "Submerged Analytics",
            controller: "AnalyticsController",
            controllerAs: "vm",
            params: { 'tab': null, 'sensor': null }
        })
            .state("control", {
            url: "/control",
            templateUrl: "app/views/control.html",
            title: "Submerged Control",
            controller: "ControlController",
            controllerAs: "vm"
        })
            .state("settings", {
            url: "/settings",
            templateUrl: "app/views/settings.html",
            title: "Submerged Settings",
            controller: "SettingsController",
            controllerAs: "vm"
        });
    }]);
var Submerged;
(function (Submerged) {
    var Services;
    (function (Services) {
        class FileService {
            constructor($q) {
                this.$q = $q;
            }
            getJsonFile(filename, folder) {
                var deferred = this.$q.defer();
                console.log(`Resolving file ${filename} in folder ${folder}`);
                window.resolveLocalFileSystemURL(folder, function (dir) {
                    dir.getFile(filename, { create: true }, function (fileEntry) {
                        // Get a File object representing the file,
                        // then use FileReader to read its contents.
                        fileEntry.file(function (file) {
                            var reader = new FileReader();
                            reader.onloadend = function (e) {
                                if (this.result.length > 0) {
                                    var obj = JSON.parse(this.result);
                                    deferred.resolve(obj);
                                }
                                else {
                                    deferred.resolve(null);
                                }
                            };
                            reader.readAsText(file);
                        }, function (err) {
                            deferred.reject(err);
                        });
                    });
                });
                return deferred.promise;
            }
            storeJsonFile(filename, folder, obj) {
                var json_data = JSON.stringify(obj);
                console.log(`Resolving file ${filename} in folder ${folder}`);
                window.resolveLocalFileSystemURL(folder, function (dir) {
                    dir.getFile(filename, { create: true }, function (fileEntry) {
                        // Create a FileWriter object for our FileEntry (log.txt).
                        fileEntry.createWriter(function (fileWriter) {
                            fileWriter.onwriteend = function (e) {
                            };
                            fileWriter.onerror = function (e) {
                                console.log(`Failed saving file ${filename}`);
                            };
                            // Create a new Blob and write it to log.txt.
                            var blob = new Blob([json_data], { type: 'text/plain' });
                            fileWriter.write(blob);
                        }, function (err) {
                            console.log(`Failed saving file ${filename}`);
                        });
                    });
                });
            }
        }
        Services.FileService = FileService;
        angular.module("ngapp").service('fileService', FileService);
    })(Services = Submerged.Services || (Submerged.Services = {}));
})(Submerged || (Submerged = {}));
var Submerged;
(function (Submerged) {
    var Services;
    (function (Services) {
        class MobileService {
            constructor(shared, fileService, $q, jwtHelper) {
                this.shared = shared;
                this.fileService = fileService;
                this.$q = $q;
                this.jwtHelper = jwtHelper;
                this.pushRegistration = null;
                this.registrationId = null;
                this.file = "authtoken.json";
                this.mobileServiceClient = new WindowsAzure.MobileServiceClient(this.shared.apiInfo.apiUrl);
                this.init();
            }
            init() {
                console.log("Initializing mobileService for communication with Azure");
                this.folder = window.cordova.file.applicationStorageDirectory;
                console.log("Initializing user data from storage.");
                this.fileService.getJsonFile(this.file, this.folder).then(function (contents) {
                    if (contents != null) {
                        console.log("Found stored auth token info, applying it to service client.");
                        this.mobileServiceClient.currentUser = {
                            userId: contents.userId,
                            mobileServiceAuthenticationToken: contents.token
                        };
                    }
                }.bind(this), function (err) {
                    console.log("Failure loading authtoken.json");
                });
            }
            saveAuthInfo() {
                console.log("mobileService.saveAuthInfo");
                var authInfo = {
                    token: this.mobileServiceClient.currentUser.mobileServiceAuthenticationToken,
                    userId: this.mobileServiceClient.currentUser.userId,
                };
                this.fileService.storeJsonFile(this.file, this.folder, authInfo);
            }
            getHeaders() {
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
            ensureLoggedIn() {
                var deferred = this.$q.defer();
                if (!this.loggedIn())
                    return this.login();
                else
                    deferred.resolve();
                return deferred.promise;
            }
            loggedIn() {
                console.log("mobileService.loggedIn");
                // If token doesn't exist, we aren't authenticated
                if (this.mobileServiceClient.currentUser === null)
                    return false;
                // Verify expiry time of current token
                var token = this.mobileServiceClient.currentUser.mobileServiceAuthenticationToken;
                var decoded = this.jwtHelper.decodeToken(token);
                if (typeof decoded.exp !== 'undefined') {
                    // exp is a UNIX Timestamp
                    var ts = Math.round((new Date()).getTime() / 1000);
                    if (ts > decoded.exp)
                        return false;
                }
                // If there isn't an expiry or we haven't expired, we are authenticated
                return true;
            }
            login(force) {
                console.log(`Performing login. Forced = ${force}`);
                var deferred = this.$q.defer();
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
                    }), function (err) {
                        console.log("Could not login into mobile services: " + err);
                        deferred.reject(err);
                        this.loggingInPromise = null;
                    }.bind(this));
                }
                else {
                    console.log("Login resolving because already logged or logging in.");
                    deferred.resolve();
                    this.loggingInPromise = null;
                }
                return deferred.promise;
            }
            initPushRegistration() {
                /**
                * Register for Push Notifications - requires the phonegap-plugin-push be installed
                */
                console.log("Initializing push notifications");
                this.pushRegistration = window.PushNotification.init({
                    android: {
                        senderID: this.shared.apiInfo.gcmSenderID
                    }
                });
                this.pushRegistration.on('registration', ((data) => {
                    this.registrationId = data.registrationId;
                    this.registerPush();
                }));
                this.pushRegistration.on('notification', ((data) => {
                    if (data && data.message && data.message.length > 0) {
                        alert(data.message);
                    }
                }));
                this.pushRegistration.on('error', ((err) => {
                    console.log('Push registration returned an error: ' + err);
                }));
            }
            registerPush() {
                console.log("Registering device for push notifications: " + this.registrationId);
                if (this.registrationId.length > 0) {
                    console.log("gcm id " + this.registrationId);
                    if (this.mobileServiceClient) {
                        console.log("registering with Azure for GCM notifications");
                        this.mobileServiceClient.push.register('gcm', this.registrationId, null, null, this.registrationCallback);
                    }
                }
            }
            unregisterPush() {
                console.log("unregistering for push messages");
                this.mobileServiceClient.push.unregister('gcm', this.registrationId);
            }
            registrationCallback() {
                console.log("registrationCallback called.");
            }
            invokeApi(apiName, options, callback) {
                this.ensureLoggedIn().then(function () {
                    this.mobileServiceClient.invokeApi(apiName, options, callback);
                }.bind(this), function () {
                    console.log("Cannot invoke API because login failed.");
                });
            }
        }
        Services.MobileService = MobileService;
        angular.module("ngapp").service('mobileService', MobileService);
    })(Services = Submerged.Services || (Submerged.Services = {}));
})(Submerged || (Submerged = {}));
"use strict";
var Submerged;
(function (Submerged) {
    var Services;
    (function (Services) {
        class ApiInfo {
        }
        Services.ApiInfo = ApiInfo;
        class DeviceInfo {
        }
        Services.DeviceInfo = DeviceInfo;
        class GlobalizationInfo {
        }
        Services.GlobalizationInfo = GlobalizationInfo;
        class Settings {
            constructor() {
            }
        }
        Services.Settings = Settings;
        class Shared {
            constructor(fileService, $q) {
                this.fileService = fileService;
                this.$q = $q;
                this.file = "settings.json";
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
            init() {
            }
            save() {
                var folder = window.cordova.file.applicationStorageDirectory;
                this.fileService.storeJsonFile(this.file, folder, this.settings);
            }
            setSettings(settings, save) {
                console.log("setSettings");
                this.settings = new Settings();
                angular.copy(settings, this.settings);
                if (save)
                    this.save();
            }
            loadSettingsFromCloud(mobileService) {
                console.log("loadSettingsFromCloud");
                var deferred = this.$q.defer();
                this.loadFromCloud(mobileService).then(((settings) => {
                    this.setSettings(settings, true);
                    deferred.resolve(settings);
                }));
                return deferred.promise;
            }
            loadSettings(mobileService) {
                return this.loadFromFile().then(((settings) => {
                    this.setSettings(settings, false);
                    return settings;
                }), ((reason) => {
                    return this.loadSettingsFromCloud(mobileService);
                }));
            }
            loadFromFile() {
                var deferred = this.$q.defer();
                var folder = window.cordova.file.applicationStorageDirectory;
                console.log("Initializing settings from storage.");
                this.fileService.getJsonFile(this.file, folder).then(function (storedSettings) {
                    if (storedSettings != null) {
                        deferred.resolve(storedSettings);
                    }
                    else {
                        deferred.reject();
                    }
                }, function () {
                    deferred.reject();
                });
                return deferred.promise;
            }
            loadFromCloud(mobileService) {
                var deferred = this.$q.defer();
                this.$q.all([
                    this.loadSensors(this.deviceInfo.deviceId, mobileService),
                    this.loadModules(this.deviceInfo.deviceId, mobileService)
                ]).then(function (values) {
                    var settings = new Settings();
                    settings.sensors = values[0];
                    settings.modules = values[1];
                    deferred.resolve(settings);
                }, function (err) {
                    deferred.reject();
                });
                return deferred.promise;
            }
            loadSensors(deviceId, mobileService) {
                var deferred = this.$q.defer();
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
            loadModules(deviceId, mobileService) {
                var deferred = this.$q.defer();
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
        Shared.$inject = ['fileService', '$q'];
        Services.Shared = Shared;
        angular.module("ngapp").service("shared", Shared);
    })(Services = Submerged.Services || (Submerged.Services = {}));
})(Submerged || (Submerged = {}));
var Submerged;
(function (Submerged) {
    var Services;
    (function (Services) {
        class SignalRService {
            constructor(mobileService, $q) {
                this.mobileService = mobileService;
                this.$q = $q;
            }
            init() {
                var deferred = this.$q.defer();
                console.log("Getting authentication headers for SignalR authentication.");
                this.mobileService.getHeaders().then(function (headers) {
                    // set bearer authentication for signalR requests
                    console.log("Setting signalR headers for authorization");
                    jQuery.signalR.ajaxDefaults.headers = headers;
                    deferred.resolve();
                }, function (error) {
                    console.log("Mobile service login failed: " + error);
                });
                return deferred.promise;
            }
        }
        Services.SignalRService = SignalRService;
        angular.module('ngapp').service('signalRService', SignalRService);
    })(Services = Submerged.Services || (Submerged.Services = {}));
})(Submerged || (Submerged = {}));
"use strict";
/// <reference path="..\..\SignalR.Client.JS\Scripts\jquery-1.6.4.js" />
/// <reference path="jquery.signalR.js" />
(function ($, window, undefined) {
    /// <param name="$" type="jQuery" />
    "use strict";
    if (typeof ($.signalR) !== "function") {
        throw new Error("SignalR: SignalR is not loaded. Please ensure jquery.signalR-x.js is referenced before ~/signalr/js.");
    }
    var signalR = $.signalR;
    function makeProxyCallback(hub, callback) {
        return function () {
            // Call the client hub method
            callback.apply(hub, $.makeArray(arguments));
        };
    }
    function registerHubProxies(instance, shouldSubscribe) {
        var key, hub, memberKey, memberValue, subscriptionMethod;
        for (key in instance) {
            if (instance.hasOwnProperty(key)) {
                hub = instance[key];
                if (!(hub.hubName)) {
                    // Not a client hub
                    continue;
                }
                if (shouldSubscribe) {
                    // We want to subscribe to the hub events
                    subscriptionMethod = hub.on;
                }
                else {
                    // We want to unsubscribe from the hub events
                    subscriptionMethod = hub.off;
                }
                // Loop through all members on the hub and find client hub functions to subscribe/unsubscribe
                for (memberKey in hub.client) {
                    if (hub.client.hasOwnProperty(memberKey)) {
                        memberValue = hub.client[memberKey];
                        if (!$.isFunction(memberValue)) {
                            // Not a client hub function
                            continue;
                        }
                        subscriptionMethod.call(hub, memberKey, makeProxyCallback(hub, memberValue));
                    }
                }
            }
        }
    }
    $.hubConnection.prototype.createHubProxies = function () {
        var proxies = {};
        this.starting(function () {
            // Register the hub proxies as subscribed
            // (instance, shouldSubscribe)
            registerHubProxies(proxies, true);
            this._registerSubscribedHubs();
        }).disconnected(function () {
            // Unsubscribe all hub proxies when we "disconnect".  This is to ensure that we do not re-add functional call backs.
            // (instance, shouldSubscribe)
            registerHubProxies(proxies, false);
        });
        proxies['liveHub'] = this.createHubProxy('liveHub');
        proxies['liveHub'].client = {};
        proxies['liveHub'].server = {
            getServerTime: function () {
                return proxies['liveHub'].invoke.apply(proxies['liveHub'], $.merge(["GetServerTime"], $.makeArray(arguments)));
            },
            sendMessage: function (name, message) {
                return proxies['liveHub'].invoke.apply(proxies['liveHub'], $.merge(["SendMessage"], $.makeArray(arguments)));
            }
        };
        return proxies;
    };
    signalR.hub = $.hubConnection("https://neptune-mobileapi.azurewebsites.net/signalr/signalr", { useDefaultPath: false });
    $.extend(signalR, signalR.hub.createHubProxies());
}($, jQuery, window));
var Submerged;
(function (Submerged) {
    var Models;
    (function (Models) {
        class AnalyticsDataModel {
        }
        Models.AnalyticsDataModel = AnalyticsDataModel;
    })(Models = Submerged.Models || (Submerged.Models = {}));
})(Submerged || (Submerged = {}));
var Submerged;
(function (Submerged) {
    var Models;
    (function (Models) {
        class ModuleModel {
        }
        Models.ModuleModel = ModuleModel;
    })(Models = Submerged.Models || (Submerged.Models = {}));
})(Submerged || (Submerged = {}));
var Submerged;
(function (Submerged) {
    var Models;
    (function (Models) {
        class RelayModel {
        }
        Models.RelayModel = RelayModel;
    })(Models = Submerged.Models || (Submerged.Models = {}));
})(Submerged || (Submerged = {}));
var Submerged;
(function (Submerged) {
    var Models;
    (function (Models) {
        class RuleModel {
        }
        Models.RuleModel = RuleModel;
    })(Models = Submerged.Models || (Submerged.Models = {}));
})(Submerged || (Submerged = {}));
var Submerged;
(function (Submerged) {
    var Models;
    (function (Models) {
        class SensorModel {
        }
        Models.SensorModel = SensorModel;
        class SensorRuleModel extends SensorModel {
        }
        Models.SensorRuleModel = SensorRuleModel;
    })(Models = Submerged.Models || (Submerged.Models = {}));
})(Submerged || (Submerged = {}));
angular.module('TimerApp', [])
    .controller('TimerCtrl', function ($scope, $timeout) {
    $scope.startTimer = function () {
        $scope.$broadcast('timer-start');
    };
    $scope.stopTimer = function () {
        $scope.$broadcast('timer-stop');
    };
    $scope.$on('timer-stopped', function (event, remaining) {
        if (remaining === 0) {
            console.log('your time ran out!');
        }
    });
})
    .directive('timer', function ($compile) {
    return {
        restrict: 'E',
        scope: false,
        replace: false,
        controller: function ($scope, $element, $timeout) {
            var timeoutId = null;
            var elem = $compile('<p>{{counter}}</p>')($scope);
            $element.append(elem);
        }
    };
});
angular.module('ngapp').directive('smLoadingIndicator', function () {
    return {
        restrict: 'EA',
        templateUrl: 'app/directives/loading-indicator.html',
        replace: true,
        transclude: true,
        scope: {
            displayText: '=smText'
        },
        link: function ($scope, element, attrs) {
        }
    };
});
angular.module("ngapp").controller("MenuController", function (shared, $state, $scope, $rootScope, $timeout, $mdSidenav) {
    var toggle = function () {
        $mdSidenav("left").toggle();
    };
    var navigate = function (route) {
        $state.go(route);
        this.toggle();
    };
    $scope.toggle = toggle;
    $scope.navigate = navigate;
    var listener = function (event, toState) {
        var title = 'Submerged';
        if (toState.title)
            title = toState.title;
        $timeout(function () {
            $scope.title = title;
        }, 0, false);
    };
    $rootScope.$on('$stateChangeSuccess', listener);
});
var Submerged;
(function (Submerged) {
    var Controllers;
    (function (Controllers) {
        class AnalyticsController {
            constructor(shared, mobileService, $scope, $stateParams, $timeout) {
                this.shared = shared;
                this.mobileService = mobileService;
                this.$scope = $scope;
                this.$stateParams = $stateParams;
                this.$timeout = $timeout;
                this.pickedDate = new Date();
                this.loadedTabData = -1;
                this.selectedTabIndex = 0;
                this.loadSensors();
                $scope.$watch(() => { return this.selectedTabIndex; }, (newValue, oldValue) => {
                    this.getData();
                });
                $scope.$watch(() => { return this.pickedDate; }, (newValue, oldValue) => {
                    this.getData();
                });
            }
            selectedTabName() {
                switch (this.selectedTabIndex) {
                    case 0:
                        return "hour";
                    case 1:
                        return "day";
                    case 2:
                        return "week";
                    case 3:
                        return "month";
                }
            }
            loadSensors() {
                var apiUrl = "sensors?deviceId=" + this.shared.deviceInfo.deviceId;
                this.mobileService.invokeApi(apiUrl, {
                    body: null,
                    method: "post"
                }, ((error, success) => {
                    if (error) {
                        // do nothing
                        console.log("Error calling /sensors to get sensors data: " + error);
                    }
                    else {
                        var sensors = success.result;
                        this.processSensors(sensors); // process the last known data for display
                        this.shared.settings.sensors = sensors;
                        this.shared.save();
                    }
                }).bind(this));
            }
            processSensors(sensors) {
                this.sensors = sensors;
            }
            chartOptions(sensor, hAxisLabel, hAxisLabels) {
                var options = {
                    isStacked: false,
                    legend: 'none',
                    title: sensor.displayName,
                    vAxis: {
                        gridlines: {
                            count: 5
                        },
                        maxValue: sensor.maxThreshold,
                        minValue: sensor.minThreshold
                    },
                    hAxis: {
                        showTextEvery: hAxisLabels,
                        title: hAxisLabel
                    }
                };
                return options;
            }
            ;
            renderChart(dataLabels, data, sensor, chartPostfix) {
                var columnName;
                var hAxisLabels;
                switch (this.selectedTabIndex) {
                    case 0:
                        columnName = "Minutes";
                        hAxisLabels = 10;
                        break;
                    case 1:
                        columnName = "Hours";
                        hAxisLabels = 2;
                        break;
                    case 2:
                        columnName = "Weekdays";
                        hAxisLabels = 2;
                        break;
                    case 3:
                        columnName = "Days";
                        hAxisLabels = 4;
                        break;
                }
                var dataTable = new google.visualization.DataTable();
                dataTable.addColumn('string', columnName);
                dataTable.addColumn('number', sensor.displayName);
                for (var i = 0; i < data.length; i++) {
                    dataTable.addRow([dataLabels[i], data[i]]);
                }
                // construct the options class for this chart
                var options = this.chartOptions(sensor, columnName, hAxisLabels);
                // construct the element id of the chart 
                var elementId = sensor.name + '_chart_' + chartPostfix;
                var element = document.getElementById(elementId);
                // construct the chart object and render the chart in the element
                var chart = new google.visualization.AreaChart(element);
                chart.draw(dataTable, options);
            }
            getData() {
                // prevent loading multiple times
                if (this.loading) {
                    console.log("Ignoring call to getData because we're already loading");
                    return;
                }
                this.loading = true;
                var date = this.pickedDate;
                var selectedTab = this.selectedTabName();
                // toISOString already converts the date 
                var resourceUri = "data/" + selectedTab + "?deviceId=" + this.shared.deviceInfo.deviceId + "&date=" + date.toISOString() + "&offset=" + date.getTimezoneOffset();
                console.log("Requesting data from back-end API");
                this.mobileService.invokeApi(resourceUri, {
                    body: null,
                    method: "post"
                }, ((error, success) => {
                    if (error) {
                        console.log("Failure getting data from API: " + error);
                    }
                    else {
                        var reportModel = success.result;
                        var columnName = "";
                        var temp1Sensor = this.sensors.firstOrDefault({ name: "temperature1" });
                        this.renderChart(reportModel.dataLabels, reportModel.dataSeries[0], temp1Sensor, selectedTab);
                        var temp2Sensor = this.sensors.firstOrDefault({ name: "temperature2" });
                        this.renderChart(reportModel.dataLabels, reportModel.dataSeries[1], temp2Sensor, selectedTab);
                        var pHSensor = this.sensors.firstOrDefault({ name: 'pH' });
                        this.renderChart(reportModel.dataLabels, reportModel.dataSeries[2], pHSensor, selectedTab);
                        console.log("data loaded, setting loadedTabData to " + this.selectedTabIndex);
                        this.loadedTabData = this.selectedTabIndex;
                        this.loading = false;
                    }
                }).bind(this));
            }
        }
        AnalyticsController.$inject = ['shared', 'mobileService', '$scope', '$stateParams', '$timeout'];
        Controllers.AnalyticsController = AnalyticsController;
        angular.module("ngapp").controller("AnalyticsController", AnalyticsController);
    })(Controllers = Submerged.Controllers || (Submerged.Controllers = {}));
})(Submerged || (Submerged = {}));
var Submerged;
(function (Submerged) {
    var Controllers;
    (function (Controllers) {
        class ControlController {
            constructor(shared, mobileService, $mdToast, $state) {
                this.shared = shared;
                this.mobileService = mobileService;
                this.$mdToast = $mdToast;
                this.$state = $state;
                var apiUrl = "control/relays?deviceId=" + this.shared.deviceInfo.deviceId;
                this.mobileService.invokeApi(apiUrl, {
                    body: null,
                    method: "post"
                }, ((error, success) => {
                    if (!error) {
                        // do nothing
                        this.processRelays(success.result);
                    }
                }).bind(this));
            }
            processRelays(relays) {
                this.relays = relays;
            }
            showSimpleToast(text) {
                this.$mdToast.show(this.$mdToast.simple()
                    .textContent(text)
                    .position("top right")
                    .hideDelay(3000));
            }
            ;
            toggle(relayNumber, relayState) {
                var apiUrl = "control/setrelay?deviceId=" + this.shared.deviceInfo.deviceId + "&relayNumber=" + relayNumber + "&state=" + relayState;
                this.mobileService.invokeApi(apiUrl, {
                    body: null,
                    method: "post"
                }, ((error, success) => {
                    if (error) {
                        // do nothing
                        this.showSimpleToast("Sorry, settings were not saved.");
                    }
                }).bind(this));
            }
            ;
        }
        Controllers.ControlController = ControlController;
        angular.module("ngapp").controller("ControlController", ControlController);
    })(Controllers = Submerged.Controllers || (Submerged.Controllers = {}));
})(Submerged || (Submerged = {}));
//"use strict";
//angular.module("ngapp").controller("ControlController", function (shared, mobileService, $mdToast, $state, $scope, $mdSidenav, $mdComponentRegistry) {
//    var vm = this;
//    vm.relays = [
//        { title: "Pump", state: true, number: 1 },
//        { title: "Heat", state: true, number: 2 },
//        { title: "Lights", state: true, number: 3 }
//    ];
//    var showSimpleToast = function (text) {
//        $mdToast.show(
//            $mdToast.simple()
//                .textContent(text)
//                .position("top right")
//                .hideDelay(3000)
//        );
//    };
//    vm.toggle = function (relayNumber, relayState) {
//        var apiUrl = "control/setrelay?deviceId=" + shared.deviceInfo.deviceId + "&relayNumber=" + relayNumber + "&state=" + relayState;
//        mobileService.invokeApi(apiUrl, {
//            body: null,
//            method: "post"
//        }, function (error, success) {
//            if (error) {
//                // do nothing
//                showSimpleToast("Sorry, settings were not saved.");
//            }
//        });
//    };
//});
var Submerged;
(function (Submerged) {
    var Controllers;
    (function (Controllers) {
        class LiveController {
            constructor(shared, mobileService, signalRService, $state, $scope, $timeout, $sce) {
                this.shared = shared;
                this.mobileService = mobileService;
                this.signalRService = signalRService;
                this.$state = $state;
                this.$scope = $scope;
                this.$timeout = $timeout;
                this.$sce = $sce;
                this.loading = true;
                this.selectedTabIndex = 0;
                this.processModules = function (modules) {
                    for (var module of modules) {
                        if (module.status != "Connected")
                            module.cssClass = "npt-kpired";
                        else
                            module.cssClass = "npt-kpigreen";
                    }
                    this.modules = modules;
                    this.loading = false;
                    this.$scope.$apply();
                };
                this.openDetails = function (item) {
                    this.$state.go('analytics', {
                        tab: "day",
                        sensor: item
                    });
                };
                // get the settings stored in local storage; when empty refresh from cloud
                var settings = shared.settings;
                if (settings.sensors == null || settings.sensors.length == 0) {
                    this.loadSensors();
                }
                else {
                    this.processSensors(settings.sensors);
                }
                $scope.$watch(() => { return this.selectedTabIndex; }, (newValue, oldValue) => {
                    this.loadData();
                });
                this.$scope.$on("$destroy", function () {
                    console.log("Stopping signalR hub since the user is leaving this view.");
                    jQuery.connection.hub.stop();
                });
                this.$scope.$on("resume", function (event, data) {
                    console.log("Received resume event in live controller");
                    // reload the data to freshen up
                    this.loadData();
                    // when the application gets resumed, we need to restart signalR
                    $.connection.hub.stop();
                    this.startSignalR();
                }.bind(this));
            }
            ;
            loadData() {
                this.loading = true;
                if (this.selectedTabIndex == 0) {
                    this.loadLatestTelemetry();
                    this.loadLastThreeHours();
                }
                else if (this.selectedTabIndex == 1) {
                    this.loadModuleData();
                }
            }
            ;
            loadSensors() {
                var apiUrl = "sensors?deviceId=" + this.shared.deviceInfo.deviceId;
                this.mobileService.invokeApi(apiUrl, {
                    body: null,
                    method: "post"
                }, ((error, success) => {
                    if (error) {
                        // do nothing
                        console.log("Error calling /sensors to get sensors data: " + error);
                    }
                    else {
                        var sensors = success.result;
                        this.processSensors(sensors); // process the last known data for display
                        this.shared.settings.sensors = sensors;
                        this.shared.save();
                    }
                }).bind(this));
            }
            formatSensorValue(sensor, value) {
                var result = "";
                if (value != null) {
                    switch (sensor.sensorType) {
                        case "temperature":
                            result = value.toFixed(1) + '&deg;';
                            break;
                        case "pH":
                            result = value.toFixed(2);
                            break;
                        default:
                            result = value.toString();
                            break;
                    }
                }
                return this.$sce.trustAsHtml(result);
            }
            loadLastThreeHours() {
                var date = new Date();
                var url = "data/threehours?deviceId=" + this.shared.deviceInfo.deviceId + "&date=" + date.toISOString() + "&offset=" + date.getTimezoneOffset();
                this.mobileService.invokeApi(url, {
                    body: null,
                    method: "post"
                }, function (error, success) {
                    if (error) {
                        // do nothing
                        console.log("Error calling /data/threehours: " + error);
                    }
                    else {
                        this.processThreeHours(success.result);
                    }
                }.bind(this));
            }
            loadLatestTelemetry() {
                // get the latest available data record to show untill it's updated
                this.mobileService.invokeApi("data/latest?deviceId=" + this.shared.deviceInfo.deviceId, {
                    body: null,
                    method: "post"
                }, function (error, success) {
                    if (error) {
                        // do nothing
                        console.log("Error calling /data/latest: " + error);
                    }
                    else {
                        this.processTelemetry(success.result); // process the last known data for display
                        this.startSignalR(); // start signalR when the data is received 
                    }
                }.bind(this));
            }
            processSensors(sensors) {
                this.sensors = sensors;
            }
            processThreeHours(data) {
                var temperature1Sensor = this.sensors.firstOrDefault({ name: "temperature1" });
                this.renderChart(data.dataLabels, data.dataSeries[0], "temperature1_chart");
                var temperature2Sensor = this.sensors.firstOrDefault({ name: "temperature2" });
                this.renderChart(data.dataLabels, data.dataSeries[1], "temperature2_chart");
                var pHSensor = this.sensors.firstOrDefault({ name: "pH" });
                this.renderChart(data.dataLabels, data.dataSeries[2], "pH_chart");
            }
            renderChart(dataLabels, data, elementId) {
                var dataTable = new google.visualization.DataTable();
                dataTable.addColumn('string');
                dataTable.addColumn('number');
                for (var i = 0; i < data.length; i++) {
                    dataTable.addRow([dataLabels[i], data[i]]);
                }
                var options = {
                    axisTitlesPosition: 'none',
                    isStacked: false,
                    displayExactValues: false,
                    curveType: 'function',
                    tooltip: {
                        trigger: 'none',
                    },
                    legend: {
                        position: 'none'
                    },
                    vAxis: {
                        gridlines: {
                            count: 2,
                        },
                    },
                    hAxis: {
                        gridlines: {
                            count: 6,
                        },
                    },
                    series: {
                        0: {
                            targetAxisIndex: 1
                        }
                    },
                    height: 45,
                    width: 200
                };
                var element = document.getElementById(elementId);
                var chart = new google.visualization.AreaChart(element);
                chart.draw(dataTable, options);
            }
            processTelemetry(data) {
                this.sensorData = data.temperature1 != null;
                this.leakData = data.leakDetected != null;
                this.loading = false;
                this.$scope.$apply();
                // when the timestamp is set (blob stored data); use it, otherwise timestamp = now
                if (data.timestamp != null)
                    this.start(data.timestamp);
                else
                    this.start(new Date().valueOf());
                for (var property in data) {
                    if (data.hasOwnProperty(property)) {
                        // find a sensor by the name of the property
                        var sensor = this.sensors.firstOrDefault({ name: property });
                        if (sensor != null) {
                            sensor.reading = data[property];
                        }
                    }
                }
            }
            ;
            loadModuleData() {
                this.loading = true;
                // get the latest available data record to show untill it's updated
                this.mobileService.invokeApi("modules?deviceId=" + this.shared.deviceInfo.deviceId, {
                    body: null,
                    method: "post"
                }, function (error, success) {
                    if (error) {
                        // do nothing
                        console.log("Error calling /modules: " + error);
                    }
                    else {
                        this.processModules(success.result); // process the last known data for display
                    }
                }.bind(this));
            }
            ;
            calculateSensorClass(sensor) {
                if (sensor != null && sensor.reading != null) {
                    // find the low and high rules for this sensor
                    var lowValue = sensor.minThreshold;
                    var highValue = sensor.maxThreshold;
                    var deviation = (highValue - lowValue) * 0.1;
                    var orangeLowValue = lowValue + deviation;
                    var orangeHighValue = highValue - deviation;
                    if (sensor.reading < orangeHighValue && sensor.reading > orangeLowValue)
                        return "md-fab npt-kpigreen";
                    else if (sensor.reading < lowValue || sensor.reading > highValue)
                        return "md-fab npt-kpired";
                    else
                        return "md-fab npt-kpiorange";
                }
                else {
                    return "md-fab npt-kpigray";
                }
            }
            startSignalR() {
                this.signalRService.init().then(function () {
                    var liveHubProxy = jQuery.connection.liveHub;
                    liveHubProxy.client.sendLiveData = ((data) => {
                        this.processData(data);
                    }).bind(this);
                    jQuery.connection.hub.start()
                        .done(function () { console.log('Now connected, connection ID=' + jQuery.connection.hub.id); })
                        .fail(function (err) {
                        console.log('Could not connect: ' + err);
                        // connecting might have failed due to expired login. Force login to refresh token,
                        // the disconnect event handler will try again after 5 seconds
                        this.mobileService.login(true);
                    });
                    // attach disconnected listener and automatically restart the connection
                    jQuery.connection.hub.disconnected(function () {
                        setTimeout(function () {
                            jQuery.connection.hub.start();
                        }, 5000); // Restart connection after 5 seconds.you can set the time based your requirement
                    });
                }.bind(this));
            }
            ;
            start(timestamp) {
                if (this.timeoutId) {
                    this.$timeout.cancel(this.timeoutId);
                }
                this.timeoutId = this.$timeout(this.onTimeout.bind(this), 1000);
                this.lastUpdated = timestamp;
            }
            ;
            stop() {
                this.$timeout.cancel(this.timeoutId);
                this.timeoutId = null;
            }
            ;
            onTimeout() {
                var dif;
                dif = new Date().valueOf() - this.lastUpdated;
                var seconds = Math.floor((dif / 1000) % 60);
                var minutes = Math.floor(((dif / (60000)) % 60));
                var hours = Math.floor(((dif / (3600000)) % 24));
                var days = Math.floor(((dif / (3600000)) / 24) % 30);
                var months = Math.floor(((dif / (3600000)) / 24 / 30) % 12);
                var years = Math.floor((dif / (3600000)) / 24 / 365);
                var text = null;
                if (years > 0)
                    text = years.toString() + " years";
                else if (months > 0)
                    text = months.toString() + " months";
                else if (days > 0)
                    text = days.toString() + " days";
                else if (hours > 0)
                    text = hours.toString() + " hours";
                else if (minutes > 0)
                    text = minutes.toString() + " minutes";
                else
                    text = seconds.toString() + " seconds";
                this.lastUpdatedText = text;
                this.timeoutId = this.$timeout(() => { this.onTimeout(); }, 1000);
            }
            ;
        }
        LiveController.$inject = ['shared', 'mobileService', 'signalRService', '$state', '$scope', '$timeout', '$sce'];
        Controllers.LiveController = LiveController;
        angular.module("ngapp").controller("LiveController", LiveController);
    })(Controllers = Submerged.Controllers || (Submerged.Controllers = {}));
})(Submerged || (Submerged = {}));
//angular.module("ngapp").controller("LiveController", function (shared, mobileService, signalr, $state, $scope, $timeout) {
//    var vm = this;
//    // initialization of the local viewmodel data
//    vm.leakDetected = false;
//    vm.leakSensors = "";
//    vm.lastUpdated = null;
//    vm.loading = true;
//    vm.selectedTabIndex = 0;
//    vm.counterStarted = false;
//    // start loading the data 
//    loadData();
//    function loadRules() {
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
//                processRules(success.result);      // process the last known data for display
//            }
//        });
//    }
//    function processRules(rules) {
//        var sensorRules = rules.where({ name: "temperature1" });
//        if (sensorRules.length == 2) {
//            vm.temperature1Low = sensorRules[0].threshold;
//            vm.temperature1High = sensorRules[1].threshold;
//        }
//        sensorRules = rules.where({ name: "temperature2" });
//        if (sensorRules.length == 2) {
//            vm.temperature2Low = sensorRules[0].threshold;
//            vm.temperature2High = sensorRules[1].threshold;
//        }
//        sensorRules = rules.where({ name: "pH" });
//        if (sensorRules.length == 2) {
//            vm.pHLow = sensorRules[0].threshold;
//            vm.pHHigh = sensorRules[1].threshold;
//        }
//    }
//    function calculateKpiClass(sensor, value) {
//        if (settings.rules != null) {
//            var sensorRules = settings.rules.where({ name: sensor });
//            // find the low and high rules for this sensor
//            var lowValue = sensorRules[0].threshold;
//            var highValue = sensorRules[1].threshold;
//            var deviation = (highValue - lowValue) * 0.1;
//            var orangeLowValue = lowValue + deviation;
//            var orangeHighValue = highValue - deviation;
//            if (value < orangeHighValue && value > orangeLowValue)
//                return "md-fab npt-kpigreen";
//            else if (value < lowValue || value > highValue)
//                return "md-fab npt-kpired";
//            else
//                return "md-fab npt-kpiorange";
//        }
//        else {
//            return "md-fab npt-kpiorange";
//        }
//    }
//    vm.getModuleCss = function (status) {
//    };
//    $scope.$on("resume", function (event, data) {
//        console.log("Received resume event in live controller");
//        // when the application gets resumed, we need to restart signalR 
//        $.connection.hub.stop();
//        startSignalR();
//        // reload the 
//        loadData();
//    })
//    $scope.$watch("vm.selectedTabIndex", function (newValue, oldValue) {
//        loadData();
//    });
//    var openDetails = function (item) {
//        $state.go('analytics', {
//            tab: "day",
//            sensor: item
//        });
//    };
//    vm.openDetails = openDetails;
//    var processModules = function (modules) {
//        for (var module of modules) {
//            if (module.status == "Disconnected")
//                module.cssClass = "npt-kpired";
//            else
//                module.cssClass = "npt-kpigreen";
//        }
//        vm.modules = modules;
//        vm.loading = false;
//        $scope.$apply();
//    }
//    var processData = function (data) {
//        // set the data object when it hasn't been set yet
//        if (data.temperature1 != null) {
//            vm.temperature1 = data.temperature1.toFixed(1);
//            vm.temperature1Class = calculateKpiClass("temperature1", vm.temperature1);
//        }
//        if (data.temperature2 != null) {
//            vm.temperature2 = data.temperature2.toFixed(1);
//            vm.temperature2Class = calculateKpiClass("temperature2", vm.temperature2);
//        }
//        if (data.pH != null) {
//            vm.pH = data.pH.toFixed(2);
//            vm.phClass = calculateKpiClass("pH", vm.pH);
//        }
//        if (data.leakDetected != null) {
//            vm.leakDetected = data.leakDetected;
//            vm.leakText = data.leakDetected ? "!!" : "OK";
//            vm.leakClass = data.leakDetected ? "md-fab npt-kpired" : "md-fab npt-kpigreen";
//            vm.leakSensors = data.leakSensors;
//        }
//        vm.sensorData = data.temperature1 != null;
//        vm.leakData = data.leakDetected != null;
//        vm.loading = false;
//        $scope.$apply();
//        // when the timestamp is set (blob stored data); use it, otherwise timestamp = now
//        if (data.timestamp != null)
//            start(data.timestamp);
//        else
//            start(new Date());
//    };
//    function loadData() {
//        if (vm.selectedTabIndex == 0)
//            loadLatestTelemetry();
//        else if (vm.selectedTabIndex == 1)
//            loadModuleData();
//    }
//    function loadModuleData() {
//        if (vm.modules != null) {
//            return;
//        }
//        else {
//            vm.loading = true;
//            // get the latest available data record to show untill it's updated
//            mobileService.invokeApi("data/modules?deviceId=" + shared.deviceInfo.deviceId, {
//                body: null,
//                method: "post"
//            }, function (error, success) {
//                if (error) {
//                    // do nothing
//                    console.log("Error calling /data/latest: " + error);
//                }
//                else {
//                    processModules(success.result);      // process the last known data for display
//                }
//            });
//        }
//    }
//    function loadLatestTelemetry() {
//        // get the latest available data record to show untill it's updated
//        mobileService.invokeApi("data/latest?deviceId=" + shared.deviceInfo.deviceId, {
//            body: null,
//            method: "post"
//        }, function (error, success) {
//            if (error) {
//                // do nothing
//                console.log("Error calling /data/latest: " + error);
//            }
//            else {
//                processData(success.result);      // process the last known data for display
//                startSignalR();         // start signalR when the data is received 
//            }
//        });
//    };
//    function startSignalR() {
//        signalr().then(function () {
//            var liveHubProxy = $.connection.liveHub;
//            liveHubProxy.client.sendLiveData = processData;
//            $.connection.hub.start()
//                .done(function () { console.log('Now connected, connection ID=' + $.connection.hub.id); })
//                .fail(function (err) {
//                    console.log('Could not connect: ' + err);
//                    // connecting might have failed due to expired login. Force login to refresh token,
//                    // the disconnect event handler will try again after 5 seconds
//                    mobileService.login(true);
//                });
//            // attach disconnected listener and automatically restart the connection
//            $.connection.hub.disconnected(function () {
//                setTimeout(function () {
//                    $.connection.hub.start();
//                }, 5000); // Restart connection after 5 seconds.you can set the time based your requirement
//            });
//            $scope.$on("$destroy", function () {
//                console.log("Stopping signalR hub since the user is leaving this view.");
//                $.connection.hub.stop();
//            });
//        });
//    }
//    var timeoutId: number = null;
//    var lastUpdated: number;
//    function start(timestamp) {
//        lastUpdated = Date.parse(timestamp);
//        if (!vm.counterStarted) {
//            timeoutId = $timeout(onTimeout, 1000);
//            vm.counterStarted = true;
//        }
//    }
//    function stop() {
//        $timeout.cancel(timeoutId);
//        vm.counterStarted = false;
//    }
//    function onTimeout() {
//        var dif: number;
//        dif = new Date().valueOf() - lastUpdated;
//        var seconds = Math.floor((dif / 1000) % 60);
//        var minutes = Math.floor(((dif / (60000)) % 60));
//        var hours = Math.floor(((dif / (3600000)) % 24));
//        var days = Math.floor(((dif / (3600000)) / 24) % 30);
//        var months = Math.floor(((dif / (3600000)) / 24 / 30) % 12);
//        var years = Math.floor((dif / (3600000)) / 24 / 365);
//        var text = null;
//        if (years > 0)
//            text = years.toString() + " years";
//        else if (months > 0)
//            text = months.toString() + " months";
//        else if (days > 0)
//            text = days.toString() + " days";
//        else if (hours > 0)
//            text = hours.toString() + " hours";
//        else if (minutes > 0)
//            text = minutes.toString() + " minutes";
//        else
//            text = seconds.toString() + " seconds";
//        vm.lastUpdated = text;
//        timeoutId = $timeout(onTimeout, 1000);
//    }
//}); 
var Submerged;
(function (Submerged) {
    var Controllers;
    (function (Controllers) {
        class LoginController {
            constructor(mobileService, $location, shared) {
                this.mobileService = mobileService;
                this.$location = $location;
                this.shared = shared;
                this.vm = this;
                this.ensureLogin().then((() => {
                    return this.loadSettings.bind(this)();
                }).bind(this), this.errorHandler).then((() => {
                    this.redirect();
                }).bind(this), this.errorHandler);
            }
            errorHandler(err) {
                console.log("Error occurred during login procedure: " + err);
            }
            ensureLogin() {
                console.log("logincontroller.ensureLogin");
                return this.mobileService.login(true);
            }
            loadSettings() {
                console.log("logincontroller.loadSettings");
                return this.shared.loadSettingsFromCloud(this.mobileService);
            }
            redirect() {
                console.log('MAIN logged in, changing path');
                this.$location.path("/live");
                this.loading = false;
            }
            login() {
                this.mobileService.login();
            }
        }
        LoginController.$inject = ['mobileService', '$location', 'shared'];
        Controllers.LoginController = LoginController;
        angular.module("ngapp").controller("LoginContoller", LoginController);
    })(Controllers = Submerged.Controllers || (Submerged.Controllers = {}));
})(Submerged || (Submerged = {}));
//angular.module("ngapp").controller("LoginContoller", function (shared, mobileService, $scope, $location) {
//    var vm = this;
//    vm.loading = true;
//    vm.login = function () {
//        mobileService.login();
//    }
//    vm.init = function () {
//    }
//    vm.init();
//});
"use strict";
var Submerged;
(function (Submerged) {
    var Controllers;
    (function (Controllers) {
        class MainController {
        }
        Controllers.MainController = MainController;
        angular.module("ngapp").controller("MainController", MainController);
    })(Controllers = Submerged.Controllers || (Submerged.Controllers = {}));
})(Submerged || (Submerged = {}));
var Submerged;
(function (Submerged) {
    var Controllers;
    (function (Controllers) {
        class SettingsController {
            constructor(shared, mobileService, fileService, $scope, $location, $mdToast) {
                this.shared = shared;
                this.mobileService = mobileService;
                this.fileService = fileService;
                this.$scope = $scope;
                this.$location = $location;
                this.$mdToast = $mdToast;
                this.loading = true;
                this.saving = false;
                this.indexedRules = [];
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
                        this.processData(success.result); // process the last known data for display
                    }
                }).bind(this));
            }
            processData(sensors) {
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
            showSimpleToast(text) {
                this.$mdToast.show(this.$mdToast.simple()
                    .textContent(text)
                    .position("top right")
                    .hideDelay(3000));
            }
            save() {
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
        Controllers.SettingsController = SettingsController;
        angular.module("ngapp").controller("SettingsController", SettingsController);
    })(Controllers = Submerged.Controllers || (Submerged.Controllers = {}));
})(Submerged || (Submerged = {}));
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
//# sourceMappingURL=appBundle.js.map