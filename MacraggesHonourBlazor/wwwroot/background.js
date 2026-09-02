// Macragge's Honour Download Interceptor Scanner
// background.js - Intercepts ALL downloads including blob URLs

chrome.runtime.onInstalled.addListener(function () {
    chrome.storage.local.set({
        masterEnabled: true,
        settings: {
            interceptRegular: true,
            interceptAuto: true,
            interceptEmail: true,
            showPopup: true,
            blockPopups: true
        }
    });
});


// =============================================
// POPUP BLOCKER
// =============================================
function writePopupLog(attemptedUrl, sourceTab, popupType) {
    chrome.storage.local.get('popupBlockLog', function (data) {
        const log = data.popupBlockLog || [];
        log.push({
            datetime: new Date().toLocaleString(),
            attemptedUrl: attemptedUrl || 'Unknown',
            sourceTab: sourceTab || 'Unknown',
            popupType: popupType
        });
        chrome.storage.local.set({ popupBlockLog: log });
    });
}

chrome.tabs.onCreated.addListener(function (tab) {
    chrome.storage.local.get(['masterEnabled', 'settings'], function (data) {
        const masterEnabled = data.masterEnabled !== false;
        const settings = data.settings || {};
        const blockPopups = settings.blockPopups !== false;

        if (!masterEnabled || !blockPopups) return;

        // Whitelist our own extension pages
        if (tab.url && tab.url.startsWith(chrome.runtime.getURL(''))) return;
        if (tab.pendingUrl && tab.pendingUrl.startsWith(chrome.runtime.getURL(''))) return;

        // Must have an opener AND a real destination URL
        if (tab.openerTabId === undefined) return;

        // Give it a tiny moment to resolve the URL
        setTimeout(() => {
            chrome.tabs.get(tab.id, function (resolvedTab) {
                if (chrome.runtime.lastError) return;

                const url = resolvedTab.url || resolvedTab.pendingUrl || '';

                // Allow blank/new tabs — these are user opened
                if (!url || url === 'about:blank' || url === 'about:newtab' || url === 'edge://newtab/') return;

                chrome.tabs.get(tab.openerTabId, function (openerTab) {
                    if (chrome.runtime.lastError) return;
                    const sourceUrl = openerTab?.url || 'Unknown';
                    chrome.tabs.remove(tab.id, function () {
                        writePopupLog(url, sourceUrl, 'tab');
                    });
                });
            });
        }, 50);
    });
});

chrome.windows.onCreated.addListener(function (win) {
    chrome.storage.local.get(['masterEnabled', 'settings'], function (data) {
        const masterEnabled = data.masterEnabled !== false;
        const settings = data.settings || {};
        const blockPopups = settings.blockPopups !== false;

        if (!masterEnabled || !blockPopups) return;

        if (win.type !== 'popup') return;

        chrome.tabs.query({ windowId: win.id }, function (tabs) {
            const winUrl = tabs[0]?.url || tabs[0]?.pendingUrl || '';

            if (winUrl.startsWith(chrome.runtime.getURL(''))) return;

            chrome.windows.remove(win.id, function () {
                writePopupLog(winUrl, 'Unknown', 'window');
            });
        });
    });
});



// Write to download log
function writeLog(downloadItem, result, scanStatus, scanMessage) {
    chrome.storage.local.get('downloadLog', function (data) {
        const log = data.downloadLog || [];
        log.push({
            datetime: new Date().toLocaleString(),
            filename: downloadItem.filename || 'Email Attachment',
            source: downloadItem.url || 'Unknown',
            isBlob: downloadItem.url.startsWith('blob:'),
            result: result,
            scanStatus: scanStatus || 'unknown',
            scanMessage: scanMessage || ''
        });
        chrome.storage.local.set({ downloadLog: log });
    });
}

chrome.downloads.onCreated.addListener(function (downloadItem) {
    chrome.storage.local.get(['masterEnabled', 'settings'], function (data) {
        const masterEnabled = data.masterEnabled !== false;
        const settings = data.settings || {};
        const interceptRegular = settings.interceptRegular !== false;
        const interceptEmail = settings.interceptEmail !== false;

        if (!masterEnabled) return;

        if (downloadItem.url.startsWith('blob:')) {
            if (!interceptEmail) return;

            chrome.storage.session.get('approvedBlob', function (approved) {
                if (approved.approvedBlob) {
                    chrome.storage.session.remove('approvedBlob');
                    writeLog(downloadItem, 'Allowed');
                    return;
                }
                chrome.downloads.cancel(downloadItem.id, function () {
                    writeLog(downloadItem, 'Intercepted', 'blob', '⚪ Email attachment');
                    chrome.storage.session.set({
                        pendingDownload: {
                            id: downloadItem.id,
                            filename: downloadItem.filename || 'Email Attachment',
                            url: downloadItem.url,
                            finalUrl: downloadItem.finalUrl,
                            isBlob: true,
                            vtResult: { status: 'blob', message: '⚪ Email attachment — cannot scan' }
                        }
                    });
                    chrome.windows.create({
                        url: chrome.runtime.getURL('index.html?route=/warning'),
                        type: 'popup',
                        width: 620,
                        height: 420
                    });
                });
            });
        } else {
            if (!interceptRegular) return;

            chrome.downloads.pause(downloadItem.id, async function () {
                writeLog(downloadItem, 'Intercepted', 'unknown', '');

                const url = downloadItem.url;

                const [headerBytes, textContent] = await Promise.all([
                    fetch(url, { headers: { 'Range': 'bytes=0-255' } })
                        .then(r => r.ok ? r.arrayBuffer() : fetch(url).then(f => f.arrayBuffer()))
                        .then(buf => Array.from(new Uint8Array(buf).slice(0, 256)))
                        .catch(() => []),
                    fetch(url, { headers: { 'Range': 'bytes=0-51200' } })
                        .then(r => r.ok ? r.text() : fetch(url).then(f => f.text()))
                        .then(text => text.substring(0, 51200))
                        .catch(() => '')
                ]);

                await chrome.storage.session.set({
                    pendingDownload: {
                        id: downloadItem.id,
                        filename: downloadItem.filename,
                        url: downloadItem.url,
                        finalUrl: downloadItem.finalUrl,
                        isBlob: false,
                        vtResult: null,
                        headerBytes: headerBytes,
                        textContent: textContent
                    }
                });

                chrome.windows.create({
                    url: chrome.runtime.getURL('index.html?route=/warning'),
                    type: 'popup',
                    width: 700,
                    height: 500
                });
            });
        }
    });
});