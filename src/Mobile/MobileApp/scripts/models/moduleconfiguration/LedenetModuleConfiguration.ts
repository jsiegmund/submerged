namespace Submerged.Models.ModuleConfiguration {

    export class LedenetPointInTime {
        time: string;
        fadeIn: number;
        level: number;
        color: string;
        white: number;
    }

    export class LedenetModuleConfigurationModel extends BaseModuleConfiguration {
        device: string;
        pointsInTime: LedenetPointInTime[] = new Array<LedenetPointInTime>();        
    }

}