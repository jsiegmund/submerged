declare var hockeyapp: HockeyApp.IHockeyApp;

// Support AMD require
declare module 'hockeyapp' {
    export = hockeyapp;
}

declare namespace HockeyApp {
   
    enum LoginMode {
        ANONYMOUS,
        EMAIL_ONLY,
        EMAIL_PASSWORD,
        VALIDATE
    }

    interface IHockeyApp {
        start(success, failure, appId, autoSend?: boolean, ignoreDefaultHandler?: boolean, loginMode?: LoginMode, appSecret?: string);
        feedback(success, failure);
        forceCrash(success, failure);
        checkForUpdate(success, failure);
        addMetaData(success, failure, data);
        trackEvent(success, failure, eventName);
    }

}