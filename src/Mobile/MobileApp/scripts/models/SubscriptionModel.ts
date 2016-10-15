namespace Submerged.Models {
    "use strict";

    export class SubscriptionModel {
        subscriptionProperties: SubscriptionPropertiesModel;
        tanks: TankModel[];
        devices: DeviceModel[];
    }
}