/// <reference path="../shared/app.ts" />
interface FileReaderEventTarget extends EventTarget {
    result: string
}

interface FileReaderEvent extends ProgressEvent {
    target: FileReaderEventTarget;
    getMessage(): string;
}

namespace Submerged.Services {
    "use strict";

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

                var errorCallback = (error) => {
                    console.log("FileService error callback on read.");
                    deferred.reject();
                };

                this.requestFileSystem().then((fs) => {
                    return this.getFileEntry(fs, filename, { create: true });
                }, errorCallback).then((fileEntry: FileEntry) => {
                    return this.getFile(fileEntry);
                }, errorCallback).then((file: File) => {
                    return this.readFile(file);
                }, errorCallback).then((obj: any) => {
                    deferred.resolve(obj);
                }, errorCallback);

            }
            else {
                deferred.resolve();
            }

            return deferred.promise;
        }

        storeJsonFile(filename: string, folder: string, obj: any): ng.IPromise<void> {
            var json_data = JSON.stringify(obj);
            var deferred = this.$q.defer<void>();

            if (!this.emulated) {

                var errorCallback = (error) => {
                    console.log("FileService error callback on write.");
                    deferred.reject();
                }

                this.requestFileSystem().then((fs) => {
                    return this.getFileEntry(fs, filename, { create: true });
                }, errorCallback).then((fileEntry) => {
                    return this.createWriter(fileEntry);
                }, errorCallback).then((writer) => {
                    return this.writeFile(writer, obj);
                }, errorCallback).then(() => {
                    deferred.resolve();
                }, errorCallback);

            }
            else {
                deferred.resolve();
            }

            return deferred.promise;
        }

        createWriter(fileEntry: FileEntry): ng.IPromise<FileWriter> {
            var deferred = this.$q.defer<FileWriter>();

            fileEntry.createWriter((writer) => {
                deferred.resolve(writer);
            }, (error: FileError) => {
                deferred.reject(error);
            });

            return deferred.promise;
        }

        writeFile(fileWriter: FileWriter, dataObj): ng.IPromise<void> {
            var deferred = this.$q.defer<void>();

            fileWriter.onwriteend = function () {
                deferred.resolve();
            };

            fileWriter.onerror = function (e) {
                deferred.reject();
            };

            fileWriter.write(dataObj);

            return deferred.promise;
        }

        readFile(file: File): ng.IPromise<any> {
            var deferred = this.$q.defer<any>();
            var reader = new FileReader();

            reader.onloadend = (event: FileReaderEvent) => {
                console.log("onloadend");
                if (event.target.result.length > 0) {
                    var obj = JSON.parse(event.target.result);
                    console.log("resolving object");
                    deferred.resolve(obj);
                }
                else {
                    console.log("resolving null");
                    deferred.resolve(null);
                }
            };

            reader.onerror = function () {
                console.log("read error");
                deferred.reject();
            };

            reader.readAsText(file);
            return deferred.promise;
        }

        getFile(fileEntry: FileEntry): ng.IPromise<File> {
            var deferred = this.$q.defer<File>();

            fileEntry.file((file) => {
                deferred.resolve(file);
            }, (error: FileError) => {
                deferred.reject(error);
            });

            return deferred.promise;
        }

        getFileEntry(fs: FileSystem, filename: string, options?: Flags): ng.IPromise<FileEntry> {
            var deferred = this.$q.defer<FileEntry>();

            fs.root.getFile(filename, options, (fileEntry) => {
                deferred.resolve(fileEntry);
            }, (error: FileError) => {
                deferred.reject(error);
            });

            return deferred.promise;
        }

        requestFileSystem(): ng.IPromise<FileSystem> {
            var deferred = this.$q.defer<FileSystem>();

            window.requestFileSystem(LocalFileSystem.PERSISTENT, 0, (fs) => {
                deferred.resolve(fs);
            }, (error: FileError) => {
                deferred.reject(error);
            });

            return deferred.promise;
        }
    }

    angular.module("ngapp").service('fileService', FileService);
}