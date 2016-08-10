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

        sensorDataPresent: boolean;
        leakDataPresent: boolean;

        modules: Submerged.Models.ModuleModel[];
        sensors: Submerged.Models.SensorModel[];

        timeoutId: any;

        deviceId: string
        //timezoneOffsetSeconds: number;

        //static $inject = ['sharedService', 'mobileService', 'signalRService', '$state', '$scope', '$timeout', '$sce'];

        constructor(private sharedService: Submerged.Services.ISharedService, private mobileService: Submerged.Services.IMobileService,
            private signalRService: Submerged.Services.ISignalRService,
            private $state: ng.ui.IState, private $scope: ng.IRootScopeService, private $timeout: ng.ITimeoutService,
            private dataService: Services.IDataService) {

            this.deviceId = sharedService.settings.getDeviceId();
            //this.timezoneOffsetSeconds = sharedService.settings.globalizationInfo.server_offset_seconds;

            // get the settings stored in local storage; when empty refresh from cloud
            var settings = sharedService.settings;
            if (settings.subscription.sensors == null || settings.subscription.sensors.length == 0) {
                this.loadSensors();
            }
            else {
                this.processSensors(settings.subscription.sensors);
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

            if (this.selectedTabIndex == 3) {
                this.loadModuleData();
            }
            else {
                this.loadLatestTelemetry();
                this.loadLastThreeHours();
            }
        };

        filterStock = function (sensor: Models.SensorModel) {
            return sensor.sensorType === Statics.SENSORTYPES.STOCKFLOAT;
        }

        filterMoisture = function (sensor: Models.SensorModel) {
            return sensor.sensorType === Statics.SENSORTYPES.MOISTURE;
        }

        filterSensors = function (sensor: Models.SensorModel) {
            return sensor.sensorType != Statics.SENSORTYPES.STOCKFLOAT &&
                   sensor.sensorType != Statics.SENSORTYPES.MOISTURE &&
                   sensor.reading != null;
        }

        loadSensors(): void {
            this.dataService.getSensors(this.deviceId).then(
                (sensors) => {
                    this.processSensors(sensors);      // process the last known data for display

                    this.sharedService.settings.subscription.sensors = sensors;
                    this.sharedService.save();
                }
            );
        }

        loadLastThreeHours(): void {
            var date = new Date();

            this.dataService.getTelemetryLastThreeHours(this.deviceId, date).then(
                (data) => {
                    this.processThreeHours(data);
                }
            );
        }

        loadLatestTelemetry(): void {
            // get the latest available data record to show untill it's updated
            this.dataService.getTelemetry(this.deviceId).then(
                (telemetry) => {
                    this.processTelemetry(telemetry);       // process the last known data for display
                    this.startSignalR();         // start signalR when the data is received 
                }
            );      
        }

        processSensors(sensors: Models.SensorModel[]): void {
            this.sensors = sensors;    
        }

        processThreeHours(data: Models.AnalyticsDataModel): void
        {
            for (var i = 0; i < data.serieLabels.length; i++) {
                var sensorName = data.serieLabels[i];
                var sensorModel: Models.SensorModel = this.sensors.firstOrDefault({ name: sensorName });
                this.renderChart(data.dataLabels, data.dataSeries[i], sensorName + "_chart");
            }
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

        processTelemetry(telemetry: Models.TelemetryModel): void {
            this.sensorDataPresent = telemetry.sensorData != null && telemetry.sensorData.length > 0;

            var leakData = telemetry.sensorData.where({ sensorName: "LeakDetected" });
            this.leakDataPresent = leakData != null && leakData.length > 0;

            this.loading = false;

            // when the timestamp is set (blob stored data); use it, otherwise timestamp = now
            if (telemetry.eventEnqueuedUTCTime != null)
                this.start(telemetry.eventEnqueuedUTCTime.valueOf());
            else
                this.start(new Date().valueOf());

            for (var sensorItem of telemetry.sensorData) {
                // find a sensor by the name of the property
                var sensor: Models.SensorModel = this.sensors.firstOrDefault({ name: sensorItem.sensorName });
                if (sensor != null) {
                    sensor.reading = sensorItem.value;
                }
            }
        };

        loadModuleData(): void {
            this.loading = true;

            this.dataService.getModules(this.deviceId).then(
                (modules) => { this.processModules(modules); }
            );
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
                    this.processTelemetry(data);
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