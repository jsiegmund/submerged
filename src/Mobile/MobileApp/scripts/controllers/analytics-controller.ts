namespace Submerged.Controllers {

    export class AnalyticsController {

        loading: boolean;

        tempChartTitle: string = "Temperature";
        pHChartTitle: string = "pH";

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

        static $inject = ['shared', 'mobileService', 'googleChartApiPromise', '$scope', '$stateParams', '$timeout'];

        constructor(private shared: Services.IShared, private mobileService: Services.IMobileService, private googleChartApiPromise: any,
            private $scope: ng.IRootScopeService, private $stateParams: ng.ui.IStateParamsService, private $timeout: ng.ITimeoutService) {

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
                    this.googleChartApiPromise.then((() => {
                        var reportModel = success.result;
                        var columnName = "";

                        switch (selectedTab) {
                            case 'hour': columnName = "Minutes"; break;
                            case 'day': columnName = "Hours"; break;
                            case 'week': columnName = "Weekdays"; break;
                            case 'month': columnName = "Days"; break;
                        }

                        // build up a datamodel having the hours and two temperature ranges in it
                        var tempData: google.visualization.DataTable = new google.visualization.DataTable();
                        tempData.addColumn('string', columnName);
                        tempData.addColumn('number', reportModel.serieLabels[0]);
                        tempData.addColumn('number', reportModel.serieLabels[1]);

                        for (var i = 0; i < reportModel.dataSeries[0].length; i++) {
                            tempData.addRow([reportModel.dataLabels[i], reportModel.dataSeries[0][i], reportModel.dataSeries[1][i]]);
                        }

                        // build up a datamodel having the hours and two temperature ranges in it
                        var pHData: google.visualization.DataTable = new google.visualization.DataTable();
                        pHData.addColumn('string', columnName);
                        pHData.addColumn('number', reportModel.serieLabels[2]);

                        for (var i = 0; i < reportModel.dataSeries[0].length; i++) {
                            pHData.addRow([reportModel.dataLabels[i], reportModel.dataSeries[2][i]]);
                        }

                        var pHChart = this.getPHChart();
                        // bind the temperature data to the chart and reset the haxis label
                        pHChart.data = pHData;
                        pHChart.options.hAxis.title = columnName;

                        var tempChart = this.getTempChart();
                        // bind the temperature data to the chart and reset the haxis label
                        tempChart.data = tempData;
                        tempChart.options.hAxis.title = columnName;

                        switch (this.selectedTabIndex) {
                            case 0:
                                this.tempChartHour = tempChart;
                                this.pHChartHour = pHChart;
                                break;
                            case 1:
                                this.tempChartDay = tempChart;
                                this.pHChartDay = pHChart;
                                break;
                            case 2:
                                this.tempChartWeek = tempChart;
                                this.pHChartWeek = pHChart;
                                break;
                            case 3:
                                this.tempChartMonth = tempChart;
                                this.pHChartMonth = pHChart;
                                break;
                        }

                        console.log("data loaded, setting loadedTabData to " + this.selectedTabIndex);
                        this.loadedTabData = this.selectedTabIndex;

                        this.loading = false;
                    }).bind(this));
                }
            }).bind(this));
        }

        getTempChart(): any {
            var tempChart: any = {};
            tempChart.type = "AreaChart";
            //tempChart.displayed = true;
            tempChart.options = {
                "title": this.tempChartTitle,
                "isStacked": "false",
                "fill": 20,
                "displayExactValues": true,
                "legend": "bottom",
                "vAxis": {
                    "gridlines": {
                        "count": 10
                    }
                },
                "hAxis": {
                    "title": "Hour"
                }
            };
            tempChart.formatters = {};

            return tempChart;
        }

        getPHChart(): any {
            var pHChart: any = {};
            pHChart.type = "AreaChart";
            //pHChart.displayed = false;
            pHChart.options = {
                "title": this.pHChartTitle,
                "isStacked": "false",
                "fill": 20,
                "displayExactValues": true,
                "legend": "none",
                "vAxis": {
                    "gridlines": {
                        "count": 10
                    }
                },
                "hAxis": {
                    "title": "Hour"
                }
            };
            pHChart.formatters = {};

            return pHChart;
        }
    }

    angular.module("ngapp").controller("AnalyticsController", AnalyticsController);
}