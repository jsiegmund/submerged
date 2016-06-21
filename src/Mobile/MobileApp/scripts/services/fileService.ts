namespace Submerged.Services {

    export interface IFileService {
        getJsonFile<T>(filename: string, folder: string): ng.IPromise<T>;
        storeJsonFile(filename: string, folder: string, obj: any): ng.IPromise<any>;
    }

    export class FileService implements IFileService {

        constructor(private $q: ng.IQService) {
        }

        getJsonFile(filename: string, folder: string): ng.IPromise<any> {
            var deferred = this.$q.defer();

            console.log(`Resolving file ${filename} in folder ${folder}`);
            window.resolveLocalFileSystemURL(folder, function (dir: any) {
                dir.getFile(filename, { create: true }, function (fileEntry: any) {
                    // Get a File object representing the file,
                    // then use FileReader to read its contents.
                    fileEntry.file(function (file: any) {
                        var reader = new FileReader();

                        reader.onloadend = function (e) {
                            if (this.result.length > 0) {
                                var obj = JSON.parse(this.result);
                                deferred.resolve(obj);
                            }
                            else {
                                deferred.resolve(null);
                            }
                        };

                        reader.readAsText(file);
                    }, function (err: any) {
                        deferred.reject(err);
                    });

                });
            });

            return deferred.promise;
        }

        storeJsonFile(filename: string, folder: string, obj: any): ng.IPromise<any> {
            var json_data = JSON.stringify(obj);
            console.log(`Resolving file ${filename} in folder ${folder}`);
            var deferred = this.$q.defer();

            window.resolveLocalFileSystemURL(folder, function (dir: DirectoryEntry) {
                dir.getFile(filename, { create: true }, function (fileEntry: FileEntry) {
                    // Create a FileWriter object for our FileEntry (log.txt).
                    fileEntry.createWriter(function (fileWriter: any) {

                        fileWriter.onwriteend = function (e: any) {

                        };

                        fileWriter.onerror = function (e: any) {
                            console.log(`Failed saving file ${filename}`);
                        };

                        // Create a new Blob and write it to log.txt.
                        var blob = new Blob([json_data], { type: 'text/plain' });

                        fileWriter.write(blob);
                        deferred.resolve();

                    }, function (error) {
                        console.log(`Failed saving file ${filename}`);
                        deferred.reject(error);
                    });
                }, (error: FileError) => { deferred.reject(error); });
            }, (error: FileError) => { deferred.reject(error); });

            return deferred.promise;
        }
    }

    angular.module("ngapp").service('fileService', FileService);
}