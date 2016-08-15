namespace Submerged.Services {

    export interface IFileService {
        getJsonFile<T>(filename: string, folder: string): ng.IPromise<T>;
        storeJsonFile(filename: string, folder: string, obj: any): ng.IPromise<void>;
    }

    export class LocalFileSystem {
        public static get PERSISTENT(): number { return 0; }
        public static get TEMPORARY(): number { return 1; }
    }

    export class FileService implements IFileService {

        emulated: boolean;

        constructor(private $q: ng.IQService, private sharedService: Services.ISharedService) {
            this.emulated = sharedService.emulated;
        }

        getJsonFile(filename: string, folder: string): ng.IPromise<any> {
            var deferred = this.$q.defer();

            if (!this.emulated) {
                console.log(`Resolving file ${filename} in folder ${folder}`);
                window.requestFileSystem(LocalFileSystem.PERSISTENT, 0, (fs) => {
                    fs.root.getFile(filename, { create: true }, (fileEntry) => {
                        fileEntry.file((file: any) => {
                            return this.readFile(fileEntry);
                        }, (err: any) => {
                            deferred.reject(err);
                        });
                    }, (err) => { deferred.reject(err); });
                });
            }
            else {
                deferred.resolve(null);
            }

            return deferred.promise;
        }

        storeJsonFile(filename: string, folder: string, obj: any): ng.IPromise<void> {
            var json_data = JSON.stringify(obj);
            console.log(`Resolving file ${filename} in folder ${folder}`);
            var deferred = this.$q.defer<void>();

            if (!this.emulated) {
                window.requestFileSystem(LocalFileSystem.PERSISTENT, 0, (fs) => {
                    fs.root.getFile(filename, { create: false }, (fileEntry) => {

                        // Create a new Blob and write it to log.txt.
                        var blob = new Blob([json_data], { type: 'text/plain' });
                        this.writeFile(fileEntry, blob);

                        deferred.resolve();

                    }, (error: FileError) => { deferred.reject(error); });
                }, (error: FileError) => { deferred.reject(error); });
            }
            else {
                deferred.resolve();
            }

            return deferred.promise;
        }

        readFile(fileEntry): ng.IPromise<any> {
            var deferred = this.$q.defer<any>();

            fileEntry.file((file) => {
                var reader = new FileReader();

                reader.onloadend = function () {
                    console.log("Successful file read: " + this.result);

                    if (this.result.length > 0) {
                        var obj = JSON.parse(this.result);
                        deferred.resolve(obj);
                    }
                    else {
                        deferred.resolve(null);
                    }
                };

                reader.readAsText(file);

            }, () => { deferred.reject; });

            return deferred.promise;
        }

        writeFile(fileEntry, dataObj): ng.IPromise<void> {
            var deferred = this.$q.defer<void>();

            // Create a FileWriter object for our FileEntry (log.txt).
            fileEntry.createWriter(function (fileWriter) {

                fileWriter.onwriteend = function () {
                    deferred.resolve();
                };

                fileWriter.onerror = function (e) {
                    deferred.reject();
                };

                // If data object is not passed in,
                // create a new Blob instead.
                if (!dataObj) {
                    dataObj = new Blob(['some file data'], { type: 'text/plain' });
                }

                fileWriter.write(dataObj);
            });

            return deferred.promise;
        }
    }

    angular.module("ngapp").service('fileService', FileService);
}