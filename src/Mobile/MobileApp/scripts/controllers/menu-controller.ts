/// <reference path="../shared/app.ts" />
namespace Submerged.Controllers {
    "use strict";

    export class MenuController {

        hideMenu: boolean = false;
        title: string;
        buttons: Submerged.Services.CommandButton[];

        constructor($rootScope: ng.IRootScopeService, private $mdSidenav: angular.material.ISidenavService,
            private menuService: Submerged.Services.IMenuService, private $location: ng.ILocationService) {

            this.title = menuService.getTitle();
            this.buttons = [];

            $rootScope.$on('sm-new-menu-data', () => {
                this.title = this.menuService.getTitle();
                this.buttons = this.menuService.getButtons();
            });

            $rootScope.$on('sm-menu-hide', () => {
                this.hideMenu = true;
            });

            $rootScope.$on('sm-menu-unhide', () => {
                this.hideMenu = false;
            });
            $rootScope.$on('$locationchangesuccess', function (event) {
                this.hideMenu = false;
            });
        }

        toggle() {
            this.$mdSidenav("left").toggle();
        }

        navigate(route: string) {
            this.$location.path(route);
            this.toggle();
        }
    }

    angular.module("ngapp").controller("MenuController", MenuController);

}