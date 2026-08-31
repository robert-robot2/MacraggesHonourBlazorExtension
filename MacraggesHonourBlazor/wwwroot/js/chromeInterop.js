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
    }
};