namespace Submerged.Models.ModuleConfiguration {
    "use strict";

    export class LedenetPointInTime {
        time: number;
        fadeIn: number;
        level: number;
        color: string;
        white: number;
    }

    export class LedenetModuleConfigurationModel extends BaseModuleConfiguration {
        device: string;
        pointsInTime: LedenetPointInTime[] = new Array<LedenetPointInTime>();        
    }

    export class LedenetPointInTimeDisplayModel extends LedenetPointInTime {
        timeString: string;
    }


}