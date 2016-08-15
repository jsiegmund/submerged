module Submerged.Statics {

    export class SENSORTYPES {
        public static get TEMPERATURE(): string { return "temperature"; }
        public static get PH(): string { return "pH"; }
        public static get STOCKFLOAT(): string { return "stockfloat"; }
        public static get FLOW(): string { return "float"; }
        public static get MOISTURE(): string { return "moisture"; }
    }

    export class MODULETYPES {
        // These link 1-on-1 to the back-end module types... any updates here
        // should be reflect there as well.
        public static get SENSORS(): string { return "SensorModule"; }
        public static get CABINET(): string { return "CabinetModule"; }
        public static get FIRMATA(): string { return "FirmataModule"; }
    }
}