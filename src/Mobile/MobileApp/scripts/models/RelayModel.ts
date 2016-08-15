namespace Submerged.Models {
    export class RelayModel {
        relayNumber: number;
        name: string;
        state: boolean;
        orderNumber: number;
        toggleForMaintenance: boolean;
        pinConfig: string[];
    }    
}