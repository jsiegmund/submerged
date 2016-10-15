namespace Submerged.Models {
    "use strict";

    export class ModuleModel {
        name: string;
        description: string;
        status: string;
        orderNumber: number;
        moduleType: string;
        configuration: any;
    }

    export class ModuleDisplayModel extends ModuleModel {
        sensorCount: number;
    }
}