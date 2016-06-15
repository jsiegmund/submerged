namespace Submerged.Models {
    export class DeviceModel {
        deviceProperties: DeviceProperties;
        modules: ModuleModel[];
        sensors: SensorModel[];
        relays: RelayModel[];
    }
}