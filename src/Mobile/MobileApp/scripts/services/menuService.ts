/// <reference path="../shared/app.ts" />
namespace Submerged.Services {
    "use strict";

    export interface IMenuService {
        getTitle(): string;
        setTitle(title: string);

        getButtons(): CommandButton[];
        setButtons(buttons: CommandButton[]);
        clearButtons(): void;
        hideMenu(): void;
        unhideMenu(): void;
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
        hide: boolean = false;

        constructor(private $rootScope: ng.IRootScopeService, private $timeout: ng.ITimeoutService) {
            this.buttons = [];
            this.title = 'Submerged';
        }

        unhideMenu(): void {
            this.$rootScope.$broadcast('sm-menu-unhide');
        }

        hideMenu(): void {
            this.$rootScope.$broadcast('sm-menu-hide');
        }

        setTitle(title: string) {
            this.title = title;
            this.$rootScope.$broadcast('sm-new-menu-data');
        }

        setButtons(buttons: CommandButton[]) {
            this.buttons = buttons;
            this.$rootScope.$broadcast('sm-new-menu-data');
        }

        getTitle() {
            return this.title;
        }

        getButtons(): CommandButton[] {
            return this.buttons;
        }

        clearButtons(): void {
            this.buttons.length = 0;
        }
    }

    angular.module("ngapp").service('menuService', MenuService);
}
