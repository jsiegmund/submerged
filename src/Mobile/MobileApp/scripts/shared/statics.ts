/// <reference path="../shared/app.ts" />
module Submerged.Statics {
    "use strict";

    export class HTTP_VERBS {
        public static get GET(): string { return "get"; }
        public static get POST(): string { return "post"; }
        public static get PUT(): string { return "put"; }
        public static get DELETE(): string { return "delete"; }
        public static get HEAD(): string { return "head"; }
        public static get OPTIONS(): string { return "options"; }
    }

    export class SENSORTYPES {
        public static get TEMPERATURE(): string { return "temperature"; }
        public static get PH(): string { return "pH"; }
        public static get STOCKFLOAT(): string { return "stockfloat"; }
        public static get FLOW(): string { return "flow"; }
        public static get MOISTURE(): string { return "moisture"; }
    }

    export class MODULETYPES {
        // These link 1-on-1 to the back-end module types... any updates here
        // should be reflect there as well.
        public static get SENSORS(): string { return "SensorModule"; }
        public static get CABINET(): string { return "CabinetModule"; }
        public static get FIRMATA(): string { return "FirmataModule"; }
        public static get LEDENET(): string { return "LedenetModule"; }
    }
}