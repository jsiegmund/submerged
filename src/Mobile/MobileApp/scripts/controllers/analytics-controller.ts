namespace Submerged.Controllers {

    export class AnalyticsController {

        loading: boolean;        
        deviceId: string;

        pickedDate: Date = new Date();
        loadedTabData: number = -1;
        selectedTabIndex: number = 0;

        tempChartHour: google.visualization.LineChart;
        pHChartHour: google.visualization.LineChart;
        tempChartDay: google.visualization.LineChart;
        pHChartDay: google.visualization.LineChart;
        tempChartWeek: google.visualization.LineChart;
        pHChartWeek: google.visualization.LineChart;
        tempChartMonth: google.visualization.LineChart;
        pHChartMonth: google.visualization.LineChart;

        sensors: Submerged.Models.SensorModel[];

        static $inject = ['shared', 'mobileService', '$scope', '$stateParams', '$timeout'];

        constructor(private shared: Services.IShared, private mobileService: Services.IMobileService,
            private $scope: ng.IRootScopeService, private $stateParams: ng.ui.IStateParamsService, private $timeout: ng.ITimeoutService) {

            this.deviceId = shared.settings.getDeviceId();
            this.loadSensors();

            $scope.$watch(() => { return this.selectedTabIndex; }, (newValue, oldValue) => {
                this.getData();
            });
            $scope.$watch(() => { return this.pickedDate; }, (newValue, oldValue) => {
                this.getData();
            });
        }

        selectedTabName(): string {
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

        loadSensors(): void {
            var apiUrl = "sensors?deviceId=" + this.deviceId;
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

                    this.shared.settings.subscription.sensors = sensors;
                    this.shared.save();
                }
            }).bind(this));
        }
        
        processSensors(sensors: Models.SensorModel[]): void {
            this.sensors = sensors;
        }

        chartOptions(sensor: Models.SensorModel, hAxisLabel: string, hAxisLabels: number): google.visualization.AreaChartOptions {
            var options = <google.visualization.AreaChartOptions>{
                isStacked: false,
                legend: 'none',
                title: sensor.displayName,
                vAxis: <google.visualization.ChartAxis>{
                    gridlines: <google.visualization.ChartGridlines>{
                        count: 5
                    },
                    maxValue: sensor.maxThreshold,
                    minValue: sensor.minThreshold
                },
                hAxis: <google.visualization.ChartAxis>{
                    showTextEvery: hAxisLabels,
                    title: hAxisLabel
                }
            };

            return options;
        };

        renderChart(dataLabels: any[], data: any[], sensor: Models.SensorModel, chartPostfix: string): void {
            var columnName: string;
            var hAxisLabels: number;

            switch (this.selectedTabIndex) {
                case 0: columnName = "Minutes"; hAxisLabels = 10; break;
                case 1: columnName = "Hours"; hAxisLabels = 2; break;
                case 2: columnName = "Weekdays"; hAxisLabels = 2; break;
                case 3: columnName = "Days"; hAxisLabels = 4; break;
            }

            var dataTable: google.visualization.DataTable = new google.visualization.DataTable();
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

        getData(): void {
            // prevent loading multiple times
            if (this.loading) {
                console.log("Ignoring call to getData because we're already loading");
                return;
            }

            this.loading = true;

            var date = this.pickedDate;
            var selectedTab = this.selectedTabName();

            // toISOString already converts the date 
            var resourceUri = "data/" + selectedTab + "?deviceId=" + this.deviceId + "&date=" + date.toISOString() + "&offset=" + date.getTimezoneOffset();

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

    angular.module("ngapp").controller("AnalyticsController", AnalyticsController);
}