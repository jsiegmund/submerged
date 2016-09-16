namespace Submerged.Services {

    export interface IModuleConfigurationService {
        selectConfig(config: Models.ModuleConfiguration.BaseModuleConfiguration);
        getSelectedConfig(): Models.ModuleConfiguration.BaseModuleConfiguration;
    }
    
}