namespace Submerged.Directives {

    interface IModuleLedenetScope extends ng.IScope {
        module: Models.ModuleModel;
        config: Models.ModuleConfiguration.LedenetModuleConfigurationModel;
    }

    class ModuleLedenetConfiguration implements ng.IDirective {
        public restrict = 'E'; //E = element, A = attribute, C = class, M = comment    
        public templateUrl = 'app/directives/module-ledenet-configuration.html';
        public replace = true;
        public transclude = true;
        public controller = Controllers.SettingsModuleLedenetController;
        public controllerAs = 'vm';
        public scope = {
            module: '='
        };

        constructor() {
            
        }

        link = ($scope: IModuleLedenetScope, element, attrs, controller: Controllers.SettingsModuleLedenetController) => {
        }

        static factory(): ng.IDirectiveFactory {
            const directive = () => new ModuleLedenetConfiguration();
            directive.$inject = [];
            return directive;
        }

    }

    angular.module('ngapp').directive('smModuleLedenetConfiguration', ModuleLedenetConfiguration.factory());;
}