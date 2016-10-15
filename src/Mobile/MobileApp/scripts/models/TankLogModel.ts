namespace Submerged.Models {
    "use strict";

    export class TankLogModel {
        logId: string;
        tankId: string;
        title: string;
        description: string;
        logType: string;
        createdTime: Date;
    }

    export class TankLogDisplayModel extends TankLogModel {
        selected: boolean;
    }


}