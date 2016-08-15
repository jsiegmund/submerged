namespace Submerged.Controllers {
    export class BaseController {

        busy: boolean = false;

        constructor(private $mdToast: ng.material.IToastService) {

        }

        showSimpleToast(text: string) {
            this.$mdToast.show(
                this.$mdToast.simple()
                    .textContent(text)
                    .position("bottom center")
                    .hideDelay(3000)
            );
        };
    }
}