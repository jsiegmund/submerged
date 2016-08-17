---
title: Hardware
date: 2016-08-17 14:49:18
---

The hardware part of submerged is currently based on two main components:

* A Raspberry Pi 2 or 3 
* Arduino (nano) modules

The RPi is your gateway to the internet, running the submerged UWP app for Windows 10 IoT. The Arduino's are the heart of *submerged modules* which you use to connect sensors, relays and other equipment that you want to control. Each Arduino is called a 'module'.

## Modules

Submerged uses 'modules' to group and connect the sensors to. A module is a logical group of sensors connected to the Pi. In the default set-up, an Arduino Nano is used for each module connected to the Pi using a Bluetooth shield. The basic module implementation at this moment uses the **Firmata** protocol to connect, but any communication type can be easily added (including a module that uses the Pi's own IO ports). 

**Example**; you can connect a 'tank' module to which you connect your temperature and pH sensors. Then a 'cabinet' module can be placed inside your cabinet. This is a logical place to connect relays, stock float sensors and moisture (leakage) sensors to. Both modules will connect to your gateway (the Pi) and all data as one package will be sent to the back-end. 

Each module is linked to a tank. This allows for even more modularity, grouping one or more modules to monitor multiple tanks at once (that part is under construction). 

## Shopping list

If you want to start using submerged, you'll need some hardware. Below is a list of the things I've used: 

### Gateway

* Raspberry Pi 2 or 3.
* WiFi dongle (in case you want to use the Pi2 via WiFi)
* Bluetooth dongle
* Optional: HDMI LCD display for the dashboard

### Sensor module

* Arduino nano
* 2x DS18B20 OneWire temperature sensor
* 1x Atlas EZO pH circuit ([link](https://www.atlas-scientific.com/product_pages/circuits/ezo_ph.html))
* Optional (but recommended): 1x Atlas circuit carrier
* pH Probe. The Atlas circuit is compatible with any standard pH probe.
* 1x HC-06 bluetooth shield
* 3x 4.7k resistor

### Gateway module

* Arduino nano
* 1x 4ch relay board 
* Nx water sensor for leak detection
* Nx float sensor for stock fluids

### Common stuff

You'll need some cable, connectors and maybe casings for your electronics, since there's water involved. I have bought:

* Generic plastic DIY project cases.
* GX12 4-pins connectors. Of course you can use any connector you like.
* A USB power hub to power the Pi, LCD and Arduino's. Of course you can also use seperate adapters for everything but then you'll need a power strip which costs about as much and takes up more space.
* Some USB cables to connect everyting.

### Future modules

For the future, I'm considering the following additional modules:

* Automatic feeding system 
* Automated water changes
* LED lighting control
You can find the fritzing diagrams for the two modules in the repository ([here](https://github.com/jsiegmund/submerged/tree/master/src/Fritzing)). These will help you with wiring everything.
