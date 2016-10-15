/// <reference path="../shared/app.ts" />
namespace Submerged.Directives {
    "use strict";

    export interface ISensorController {
        formatSensorValue(sensor: Models.SensorModel, value: any): any;
        calculateSensorClass(sensor: Models.SensorModel): string;
        openDetails(sensor: Models.SensorModel): void;
        renderChart(dataLabels: any[], data: any[], elementId: string): void;
    }

    export class SensorController implements ISensorController {
        //static $inject = ['$sce'];

        public sensors: Models.SensorModel[];
        private deviceId: string;

        constructor(private $sce: ng.ISCEService, private $state: ng.ui.IStateService, private telemetryService: Services.ITelemetryService) {
            // $location and toaster are now properties of the controller
        }

        openDetails = (sensor: Models.SensorModel) => {
            this.$state.go('analytics', {
                tab: "day",
                sensor: sensor
            });
        };

        formatSensorValue = (sensor: Models.SensorModel, value: any): any => {
            var result: string = "";
            switch (sensor.sensorType) {
                case "temperature":
                    result = value.toFixed(1) + '&deg;';
                    break;
                case "pH":
                    result = value.toFixed(2);
                    break;
                case "stockfloat":
                    result = value != true ? "LEVEL OK" : "LEVEL LOW";
                    break;
                case "moisture":
                    result = value != true ? "DRY" : "WET";
                    break;
                default:
                    result = value.toString();
                    break;
            }

            return this.$sce.trustAsHtml(result);
        }

        calculateSensorClass = (sensor: Models.SensorModel): string => {
            switch (sensor.sensorType) {
                case Statics.SENSORTYPES.FLOW:
                    return this.calculateSensorClassByThreshold(sensor);
                case Statics.SENSORTYPES.MOISTURE:
                    return this.calculateSensorClassByBool(sensor, false, true);
                case Statics.SENSORTYPES.PH:
                    return this.calculateSensorClassByThreshold(sensor);
                case Statics.SENSORTYPES.STOCKFLOAT:
                    return this.calculateSensorClassByBool(sensor, false, true);
                case Statics.SENSORTYPES.TEMPERATURE:
                    return this.calculateSensorClassByThreshold(sensor);
            }
        }

        calculateSensorClassByBool = (sensor: Models.SensorModel, okState: boolean, nullIsOk: boolean): string => {
            var result: boolean;

            if (sensor.reading === undefined || sensor.reading === null)
                result = nullIsOk;
            else
                result = sensor.reading === okState;

            if (result)
                return "npt-kpigreen";
            else
                return "npt-kpired";
        }

        calculateSensorClassByThreshold = (sensor: Models.SensorModel): string => {
            if (sensor === null) {
                return "md-fab npt-kpigray";
            }

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

        renderChart = (dataLabels: any[], data: any[], elementId: string): void => {

            var dataTable: google.visualization.DataTable = new google.visualization.DataTable();
            dataTable.addColumn('string');
            dataTable.addColumn('number');

            for (var i = 0; i < data.length; i++) {
                dataTable.addRow([dataLabels[i], data[i]]);
            }

            var options = <google.visualization.AreaChartOptions>{
                axisTitlesPosition: 'none',
                isStacked: false,
                displayExactValues: false,
                curveType: 'function',
                tooltip: <google.visualization.ChartTooltip>{
                    trigger: 'none',
                },
                legend: <google.visualization.ChartLegend>{
                    position: 'none'
                },
                vAxis: <google.visualization.ChartAxis>{
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
                chartArea: {
                    width: '85%',
                    left: 0
                },
                height: 45,
                width: 200
            };

            var element = document.getElementById(elementId);
            var chart = new google.visualization.AreaChart(element);
            chart.draw(dataTable, options);
        }


    }
}