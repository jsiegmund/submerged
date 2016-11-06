namespace Submerged.Models {
    "use strict";

    export class SensorModel {
        name: string;
        displayName: string;
        description: string;
        orderNumber: number;
        minThreshold: number;
        maxThreshold: number;
        minThresholdEnabled: boolean;
        maxThresholdEnabled: boolean;
        reading: any;
        readingFormatted: string;
        sensorType: string;
        pinConfig: string[];
    }

    export class SensorRuleModel extends SensorModel {
        minimumValue: number;
        maximumValue: number;
        step: number;
    }

}