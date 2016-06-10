﻿interface JQueryStatic {
    signalR: any;
}

namespace Submerged.Services {

    export interface ISignalRService {
        init(): ng.IPromise<{}>;
    }

    export class SignalRService {

        constructor(private mobileService: IMobileService, private $q: ng.IQService) {
        }

        init(): ng.IPromise<{}> {
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

    angular.module('ngapp').service('signalRService', SignalRService);

}

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

} ($, jQuery, window));