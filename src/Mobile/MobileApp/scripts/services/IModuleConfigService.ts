namespace Submerged.Services {
    "use strict";

    export interface IModuleConfigurationService {
        selectConfig(config: Models.ModuleConfiguration.BaseModuleConfiguration);
        getSelectedConfig(): Models.ModuleConfiguration.BaseModuleConfiguration;
    }
    
}