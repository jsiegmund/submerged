namespace Submerged.Controllers {

    export class MenuController {

        title: string;
        buttons: Submerged.Services.CommandButton[];

        constructor($rootScope: ng.IRootScopeService, private $mdSidenav: angular.material.ISidenavService,
            private menuService: Submerged.Services.IMenuService, private $location: ng.ILocationService) {

            this.title = menuService.getTitle();
            this.buttons = [];

            $rootScope.$on('new-menu-data', () => {
                this.title = this.menuService.getTitle();
                this.buttons = this.menuService.getButtons();
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