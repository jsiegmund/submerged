namespace Submerged.Models {
    export class SubscriptionModel {
        subscriptionProperties: SubscriptionPropertiesModel;
        tanks: TankModel[];
        devices: DeviceModel[];
        sensors: SensorModel[];
        modules: ModuleModel[];
    }
}