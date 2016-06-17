# Hardware

The hardware part of submerged is currently based on two main components: 

* A Raspberry Pi 2 or 3 
* Arduino (nano) modules

The RPi is your gateway to the internet, running the submerged UWP app for Windows 10 IoT. The Arduino's are the heart of *submerged modules* which you use to connect sensors, relays and other equipment that you want to control. Each Arduino is called a 'module'.

## Modules

At the moment, submerged supports two types of modules: 

* **Sensor**: as you might guess, this modules connects different sensors to the system. For my set-up I'm using a pH sensor and two temperature sensors.
* **Cabinet**: this module belongs in your cabinet. It connects water leakage sensors, relays for switching power outlets and float switches to signal you when your stock fluids are running out.

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
* 1x Atlas EZO pH circuit ([link](https://www.atlas-scientific.com/product_pages/circuits/ezo_ph.html]))
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

You can find the fritzing diagrams for the two modules in the repository ([here](https://github.com/jsiegmund/submerged/tree/master/src/Fritzing)). These will help you with wiring everything.
