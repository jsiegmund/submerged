namespace Submerged.Models {
    export class SubscriptionModel {
        subscriptionProperties: SubscriptionPropertiesModel;
        tanks: TankModel[];
        devices: DeviceModel[];
        //sensors: SensorModel[];
        //modules: ModuleModel[];

        getAllSensors(): SensorModel[] {
            var result: SensorModel[] = new Array<SensorModel>();

            for (var device of this.devices) {
                result = result.concat(device.sensors);
            }

            return result;
        }

        getAllModules(): ModuleModel[] {
            var result: ModuleModel[] = new Array<ModuleModel>();

            for (var device of this.devices) { 
                result = result.concat(device.modules);
            }

            return result;
        }

        getAllRelays(): RelayModel[] {
            var result: RelayModel[] = new Array<RelayModel>();

            for (var device of this.devices) {
                result = result.concat(device.relays);
            }

            return result;
        }
    }
}