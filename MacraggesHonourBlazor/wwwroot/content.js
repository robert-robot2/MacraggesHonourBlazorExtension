// Macragge's Honour - Content Script
// Runs inside Outlook/Gmail page context
// Can access blob URLs created by the page

chrome.runtime.onMessage.addListener(function (message, sender, sendResponse) {
    if (message.action === 'redownloadBlob') {
        const blobUrl = message.blobUrl;
        const filename = message.filename || 'email-attachment';

        try {
            // Fetch the blob from page context where it's still alive
            fetch(blobUrl)
                .then(response => response.blob())
                .then(blob => {
                    // Create a new blob URL from the fetched blob
                    const newBlobUrl = URL.createObjectURL(blob);

                    // Create a temporary link and click it to trigger download
                    const link = document.createElement('a');
                    link.href = newBlobUrl;
                    link.download = filename;
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);

                    // Clean up
                    setTimeout(() => URL.revokeObjectURL(newBlobUrl), 1000);
                    sendResponse({ success: true });
                })
                .catch(err => {
                    console.error('Blob re-fetch failed:', err);
                    sendResponse({ success: false });
                });
        } catch (err) {
            console.error('Content script error:', err);
            sendResponse({ success: false });
        }
        return true; // Keep message channel open for async response
    }
});
