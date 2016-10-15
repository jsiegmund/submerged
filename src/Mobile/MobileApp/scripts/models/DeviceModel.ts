namespace Submerged.Models {
    "use strict";

    export class DeviceModel {
        deviceProperties: DeviceProperties;
        modules: ModuleModel[];
        sensors: SensorModel[];
        relays: RelayModel[];

        constructor() {
            this.deviceProperties = new DeviceProperties();
        }
    }

    export class DeviceDisplayModel extends Models.DeviceModel {
        selected: boolean;
    }

}