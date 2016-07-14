namespace Submerged.Services {

    export interface IMenuService {
        getTitle(): string;
        setTitle(title: string);

        getButtons(): CommandButton[];
        setButtons(buttons: CommandButton[]);
    }

    export class CommandButton {
        svgSrc: string;
        clickHandler: () => void;
        label: string;
        owner: any;

        click() {
            this.clickHandler.apply(this.owner); 
        }
    }

    export class MenuService implements IMenuService {

        buttons: CommandButton[];
        title: string;

        constructor(private $rootScope: ng.IRootScopeService, private $timeout: ng.ITimeoutService) {
            this.buttons = [];
            this.title = 'Submerged';
        }

        setTitle(title: string) {
            this.title = title;
            this.$rootScope.$broadcast('new-menu-data');
        }

        setButtons(buttons: CommandButton[]) {
            this.buttons = buttons;
            this.$rootScope.$broadcast('new-menu-data');
        }

        getTitle() {
            return this.title;
        }

        getButtons(): CommandButton[] {
            return this.buttons;
        }
    }

    angular.module("ngapp").service('menuService', MenuService);
}
