namespace Submerged.Models {
    export class SubscriptionModel {
        subscriptionProperties: SubscriptionPropertiesModel;
        tanks: TankModel[];
        devices: DeviceModel[];
    }
}