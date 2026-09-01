
// chromeInterop.js

window.chromeInterop = {
    getSettings: function () {
        return new Promise(resolve => {
            chrome.storage.local.get(['masterEnabled', 'settings'], function (data) {
                const s = data.settings || {};
                resolve({
                    masterEnabled: data.masterEnabled !== false,
                    interceptRegular: s.interceptRegular !== false,
                    interceptAuto: s.interceptAuto !== false,
                    interceptEmail: s.interceptEmail !== false,
                    showPopup: s.showPopup !== false
                });
            });
        });
    },
    saveSettings: function (settings) {
        return new Promise(resolve => {
            chrome.storage.local.set({
                masterEnabled: settings.masterEnabled,
                settings: {
                    interceptRegular: settings.interceptRegular,
                    interceptAuto: settings.interceptAuto,
                    interceptEmail: settings.interceptEmail,
                    showPopup: settings.showPopup
                }
            }, resolve);
        });
    },
    getLog: function () {
        return new Promise(resolve => {
            chrome.storage.local.get('downloadLog', function (data) {
                resolve(data.downloadLog || []);
            });
        });
    },
    clearLog: function () {
        return new Promise(resolve => {
            chrome.storage.local.set({ downloadLog: [] }, resolve);
        });
    },
    getProviders: function () {
        return new Promise(resolve => {
            chrome.storage.local.get('providers', function (data) {
                resolve(data.providers || [
                    'outlook.live.com',
                    'mail.google.com',
                    'mail.proton.me'
                ]);
            });
        });
    },
    // TODO: Wire up to chrome.scripting.registerContentScripts() for dynamic provider support
    addProvider: function (provider) {
        return new Promise(resolve => {
            chrome.storage.local.get('providers', function (data) {
                const providers = data.providers || [
                    'outlook.live.com',
                    'mail.google.com',
                    'mail.proton.me'
                ];
                if (!providers.includes(provider)) {
                    providers.push(provider);
                    chrome.storage.local.set({ providers }, resolve);
                } else {
                    resolve();
                }
            });
        });
    },
    // TODO: Wire up to chrome.scripting.unregisterContentScripts() for dynamic provider support
    removeProvider: function (provider) {
        return new Promise(resolve => {
            chrome.storage.local.get('providers', function (data) {
                const providers = (data.providers || []).filter(p => p !== provider);
                chrome.storage.local.set({ providers }, resolve);
            });
        });
    },
    restoreDefaults: function () {
        return new Promise(resolve => {
            chrome.storage.local.set({
                providers: [
                    'outlook.live.com',
                    'mail.google.com',
                    'mail.proton.me'
                ]
            }, resolve);
        });
    }
};