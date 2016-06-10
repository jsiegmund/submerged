(function () {
    'use strict';

    function bootstrapApplication() {
        angular.bootstrap(document, ['ngapp']);
    }

    function initialize() {
        if (window.cordova) {
            document.addEventListener('deviceready', bootstrapApplication, false);
        } else {
            bootstrapApplication();
        }
    }

    initialize();

})();