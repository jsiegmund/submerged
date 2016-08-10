namespace Submerged.Models {
    export class TelemetryModel {
        deviceId: string;
        eventEnqueuedUTCTime: Date;
        sensorData: TelemetrySensorModel[];
    }

    export class TelemetrySensorModel {
        sensorName: string;
        value: any;
    }
}