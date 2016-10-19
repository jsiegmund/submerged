/// <reference path="../shared/app.ts" />
namespace Submerged.Services {
    "use strict";

    export interface ISignalRService {
        init(callback: (data: any) => void): ng.IPromise<{}>;
        stop();
    }

    export class SignalRService {

        _started: boolean = false;

        constructor(private mobileService: IMobileService, private $q: ng.IQService) {

        }

        init(callback: (data: any) => void): ng.IPromise<{}> {
            var deferred = this.$q.defer();

            this._started = true;

            var errorCallback = (err) => {
                console.log("SignalR connection failed: " + err);
                deferred.reject();
            }

            // add the disconnected callback to handle disconnects
            jQuery.connection.hub.disconnected(this.hubDisconnected);
            jQuery.connection.hub.transportConnectTimeout = 3000;

            this.mobileService.getHeaders().then((headers) => {
                // attach the callback
                var liveHubProxy = jQuery.connection.liveHub;
                liveHubProxy.client.sendLiveData = (data) => {
                    try {
                        callback(data);
                    } catch (e) {
                        console.log("Failure processing signalR callback. Silently continuing.");
                    }
                }
                return this.startHub(headers);
            }, errorCallback).then(() => {
                console.log('Now connected, connection ID=' + jQuery.connection.hub.id);
                deferred.resolve();
            }, errorCallback);

            return deferred.promise;
        }

        stop() {
            jQuery.connection.hub.stop(false, true);
            this._started = false;
        }

        hubDisconnected() { 
            if (this._started) {
                setTimeout(() => {
                    jQuery.connection.hub.start();
                }, 5000); // Restart connection after 5 seconds.you can set the time based your requirement
            }
        }

        startHub(headers: any): ng.IPromise < void> {
            var deferred = this.$q.defer<void>();
            
            // set bearer authentication for signalR requests
            jQuery.signalR.ajaxDefaults.headers = headers;
            jQuery.connection.hub.logging = true;

            jQuery.connection.hub.start(<SignalR.ConnectionOptions>{ jsonp: true })
                  .done(() => { deferred.resolve(); })
                  .fail((err) => { deferred.reject(err); });

            return deferred.promise;            
        }
    }

    angular.module('ngapp').service('signalRService', SignalRService);

    function registerHubProxies() {
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
                    } else {
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
    }

    registerHubProxies();
}