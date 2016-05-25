
# submerged

Submerged is an opensource .NET based framework for connecting your aquarium to the IoT. The full solution consists of:

* An Azure based IoT back-end which collects data and hosts an API
* Arduino devices connected to a Raspberry Pi running Windows IoT Core via Bluetooth
* An Apache Cordova mobile app for mobile phones

# What does it do

Submerged is meant to help you monitor your aquarium in every way you want to. At the moment, I use it to monitor temperature, pH and leakage. Based on those readings you can set triggers so your phone notifies you as soon as things go south for whatever reason. It's also possible to control hardware such as relays to switch power sockets on / off. And I'm planning on adding a lot more functionality such as an intelligent log which alerts you when your stock solutions or CO2 bottle need replacing.

# How do I use it

At the moment, the sources are here for you to use under the Apache 2.0 License. To run the entire solution, you'll need both the hardware (which I will detail below) as well as an Azure subscription to run the back-end. I'm planning on offering a multi-tenant back-end against a small fee (just to cover the costs), let me know when you're interested in this. More information on how to build the entire solution can be found here: <http://blog.repsaj.nl/index.php/series/azure-aquarium-monitor/>.