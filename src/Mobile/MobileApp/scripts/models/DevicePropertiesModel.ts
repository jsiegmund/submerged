namespace Submerged.Models {
    "use strict";

    export class DeviceProperties {
        deviceID: string;
        createdTime: Date;
        updatedTime: Date;
        isSimulatedDevice: boolean;
        orderNumber: number;
        primaryKey: string;
        secondaryKey: string;
        isInMaintenance: boolean;
    }
}