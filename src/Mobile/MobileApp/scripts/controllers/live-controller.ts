interface JQueryStatic {
    connection: any;
}

namespace Submerged.Controllers {

    export class LiveController {

        leakDetected: boolean;
        leakSensors: string;
        loading: boolean = true;
        selectedTabIndex: number = 0;
        counterStarted: boolean;
        lastUpdated: number;
        lastUpdatedText: string;

        temperature1: number;
        temperature1Low: number;
        temperature1High: number;
        temperature1Class: string;

        temperature2: number;
        temperature2Low: number;
        temperature2High: number;
        temperature2Class: string;

        pH: number;
        pHLow: number;
        pHHigh: number;
        pHClass: string;

        leakText: string;
        leakClass: string;

        sensorData: boolean;
        leakData: boolean;

        modules: Submerged.Models.ModuleModel[];
        sensors: Submerged.Models.SensorModel[];

        timeoutId: any;

        static $inject = ['shared', 'mobileService', 'signalRService', '$state', '$scope', '$timeout', '$sce'];

        constructor(private shared: Submerged.Services.IShared, private mobileService: Submerged.Services.IMobileService,
            private signalRService: Submerged.Services.ISignalRService,
            private $state: ng.ui.IState, private $scope: ng.IRootScopeService, private $timeout: ng.ITimeoutService,
            private $sce: ng.ISCEService) {

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
            }.bind(this))
        };

        loadData(): void {
            this.loading = true;

            if (this.selectedTabIndex == 0) {
                this.loadLatestTelemetry();
                this.loadLastThreeHours();
            }
            else if (this.selectedTabIndex == 1) {
                this.loadModuleData();
            }
        };

        loadSensors(): void {
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
                    var sensors: Models.SensorModel[] = success.result;
                    this.processSensors(sensors);      // process the last known data for display

                    this.shared.settings.sensors = sensors;
                    this.shared.save();    
                }
            }).bind(this));
        }

        formatSensorValue(sensor: Models.SensorModel, value: any): any {
            var result: string = "";
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

        loadLastThreeHours(): void {
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

        loadLatestTelemetry(): void {
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
                    this.processTelemetry(success.result);      // process the last known data for display
                    this.startSignalR();         // start signalR when the data is received 
                }
            }.bind(this));
        }

        processSensors(sensors: Models.SensorModel[]): void {
            this.sensors = sensors;    
        }

        processThreeHours(data: Models.AnalyticsDataModel): void
        {
            var temperature1Sensor: Models.SensorModel = this.sensors.firstOrDefault({ name: "temperature1" });
            this.renderChart(data.dataLabels, data.dataSeries[0], "temperature1_chart");

            var temperature2Sensor: Models.SensorModel = this.sensors.firstOrDefault({ name: "temperature2" });
            this.renderChart(data.dataLabels, data.dataSeries[1], "temperature2_chart");

            var pHSensor: Models.SensorModel = this.sensors.firstOrDefault({ name: "pH" });
            this.renderChart(data.dataLabels, data.dataSeries[2], "pH_chart");
        }

        renderChart(dataLabels: any[], data: any[], elementId: string): void {

            var dataTable: google.visualization.DataTable = new google.visualization.DataTable();
            dataTable.addColumn('string');
            dataTable.addColumn('number');

            for (var i = 0; i < data.length; i++) {
                dataTable.addRow([dataLabels[i], data[i]]);
            }

            var options = <google.visualization.AreaChartOptions> {
                axisTitlesPosition: 'none',
                isStacked: false,
                displayExactValues: false,
                curveType: 'function',
                tooltip: <google.visualization.ChartTooltip> {
                    trigger: 'none',
                },
                legend: <google.visualization.ChartLegend> {
                    position: 'none'
                },
                vAxis: <google.visualization.ChartAxis> {
                    gridlines: {
                        count: 2,
                    },
                },
                hAxis: <google.visualization.ChartAxis>{
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

        processTelemetry(data): void {
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
                    var sensor: Models.SensorModel = this.sensors.firstOrDefault({ name: property });
                    if (sensor != null) {
                        sensor.reading = data[property];
                    }
                }
            }
        };

        loadModuleData(): void {

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
                    this.processModules(success.result);      // process the last known data for display
                }
            }.bind(this));

        };

        processModules = function (modules) {
            for (var module of modules) {
                if (module.status != "Connected")
                    module.cssClass = "npt-kpired";
                else
                    module.cssClass = "npt-kpigreen";
            }

            this.modules = modules;

            this.loading = false;
            this.$scope.$apply();
        }

        calculateSensorClass(sensor: Models.SensorModel) {
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

        openDetails = function (item) {
            this.$state.go('analytics', {
                tab: "day",
                sensor: item
            });
        };

        startSignalR(): void {
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
        };

        start(timestamp: number): void {
            if (this.timeoutId) {
                this.$timeout.cancel(this.timeoutId);
            }

            this.timeoutId = this.$timeout(this.onTimeout.bind(this), 1000);
            this.lastUpdated = timestamp;
        };

        stop(): void {
            this.$timeout.cancel(this.timeoutId);
            this.timeoutId = null;
        };

        onTimeout(): void {
            var dif: number;
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
        };
    }

    angular.module("ngapp").controller("LiveController", LiveController);
}

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