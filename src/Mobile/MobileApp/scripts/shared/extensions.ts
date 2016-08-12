interface Array<T> {
    select(expr: any): any;
    where(filter: any): Array<T>;
    firstOrDefault(func: any): T;
    first(): T;
    any(filter: any): boolean;
}

Array.prototype.select = function (expr: any) {
    var arr = this;
    switch (typeof expr) {
        case 'function':
            return jQuery.map(arr, expr);
        case 'string':
            try {
                var func:any = new Function(expr.split('.')[0], 'return ' + expr + ';');
                return jQuery.map(arr, func);

            } catch (e) {
                return null;
            }
        default:
            throw new ReferenceError('expr not defined or not supported');
    }
};


Array.prototype.where = function (filter: any) {
    var collection = this;
    switch (typeof filter) {
        case 'function':
            return jQuery.grep(collection, filter);
        case 'object':
            for (var property in filter) {
                if (!filter.hasOwnProperty(property))
                    continue; // ignore inherited properties
                collection = jQuery.grep(collection, function (item: any) {
                    return item[property] === filter[property];
                });
            }
            return collection.slice(0); // copy the array 
        // (in case of empty object filter)
        default:
            throw new TypeError('func must be either a' +
                'function or an object of properties and values to filter by');
    }
};

Array.prototype.any = function (filter: any): boolean {
    var collection = this;
    return this.where(filter).length > 0;
};

Array.prototype.firstOrDefault = function (func: any) {
    return this.where(func)[0] || null;
};

Array.prototype.first = function () {
    return this[0] || null;
};