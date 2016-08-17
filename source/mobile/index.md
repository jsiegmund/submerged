---
title: mobile
date: 2016-08-17 15:58:49
---

The mobile app is what you'll use to monitor your tank, receive notifications and control the modules you've got connected.

## Setting up
The mobile app allows you to configure your submerged set-up. The following can be configured: 

* tanks
* sensors
* relays
* modules

## Configure a tank
Use the app to add a new tank to your set-up. Modules are linked to a tank, which automatically also links everything connected to a module to the same tank. This allows you to use submerged for multiple tanks with only one single submerged gateway. The modules are cheap so there's little reason not to use multiple ones.

## Configure a sensor
When configuring a new sensor, the sensor type determines the options you'll see. Configurable options are:

* **Display name**: set the display name. This is used in both the app and the gateway dashboard when used.
* **Description**: provide an additional description. For instance "In tank temperature" if you've got temperature sensors inside and outside of your tank.
* **Type**: choose the sensor type. Check out ([this page](/sensor-support)) to see what is supported.
* **Pin configuration**: select the pin to which this sensor is physically connected.
* **Order number**: this will be replaced in the future by drag & drop, for now use it to set the display order of the sensors.

<img src="http://www.repsaj.nl/Uploads/submerged/submerged_app_sensor.png" alt="Screenshot Live" width="400"/>

## Configure a relay
Relay configuration is a bit simpler having less options.

<img src="http://www.repsaj.nl/Uploads/submerged/submerged_app_relay.png" alt="Screenshot Live" width="400"/>

## Configure a module

## How it works
The mobile app has been designed to provide simple interfaces for you to administer submerged. To use it, you will need an account. Contact me on twitter ([@jsiegmund](http://www.twitter.com/jsiegmund)) to get that going. 

## Supported platforms
The mobile app is based on Apache Cordova, which means it will run on all major operating systems and across devices. For more information, see here: https://cordova.apache.org/docs/en/latest/guide/support/.

At the moment though, only an **Android** implementation is provided, which is being tested on Android version 6. Feel free to submit a PR enabling other platforms as well.