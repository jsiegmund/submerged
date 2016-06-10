interface Window {
    plugins: Plugins;
}

interface Plugins {
    orientationLock: OrientationLock
}

/**
 * This plugin allows to receive push notifications. The Android implementation uses
 * Google's GCM (Google Cloud Messaging) service,
 * whereas the iOS version is based on Apple APNS Notifications
 */
interface OrientationLock {
    lock(type: string);
}