// Macragge's Honour - File Scanner
// fileScanner.js - Handles file byte reading and content extraction
// Passes data to Blazor FileScannerService for analysis

window.fileScanner = {

    // =============================================
    // READ FILE HEADER BYTES
    // =============================================
    readHeaderBytes: async function (url) {
        try {
            // Only fetch first 256 bytes — enough for magic number detection
            const response = await fetch(url, {
                headers: { 'Range': 'bytes=0-255' }
            });

            if (!response.ok) {
                // Fallback — fetch full response if range not supported
                const fallback = await fetch(url);
                if (!fallback.ok) return null;
                const buffer = await fallback.arrayBuffer();
                const bytes = new Uint8Array(buffer).slice(0, 256);
                return Array.from(bytes);
            }

            const buffer = await response.arrayBuffer();
            const bytes = new Uint8Array(buffer).slice(0, 256);
            return Array.from(bytes);

        } catch (error) {
            console.warn('fileScanner: header read failed —', error);
            return null;
        }
    },

    // =============================================
    // READ TEXT CONTENT
    // =============================================
    readTextContent: async function (url) {
        try {
            // Only fetch first 50KB of text — enough for pattern scanning
            const response = await fetch(url, {
                headers: { 'Range': 'bytes=0-51200' }
            });

            if (!response.ok) {
                const fallback = await fetch(url);
                if (!fallback.ok) return null;
                const text = await fallback.text();
                return text.substring(0, 51200);
            }

            const text = await response.text();
            return text;

        } catch (error) {
            console.warn('fileScanner: text read failed —', error);
            return null;
        }
    },

    // =============================================
    // FULL SCAN DATA FETCH
    // Fetches both header bytes and text content
    // Returns combined payload for Blazor to process
    // =============================================
    fetchScanData: async function (url, filename) {
        try {
            if (!url || url.startsWith('blob:')) {
                return {
                    success: false,
                    reason: 'blob',
                    headerBytes: [],
                    textContent: ''
                };
            }
            // Skip Google/Gmail attachment URLs — CORS blocked
            if (url.includes('mail-attachment.googleusercontent.com') ||
                url.includes('mail.google.com')) {
                return {
                    success: false,
                    reason: 'cors-blocked',
                    headerBytes: [],
                    textContent: ''
                };
            }
            const [headerBytes, textContent] = await Promise.all([
                window.fileScanner.readHeaderBytes(url),
                window.fileScanner.readTextContent(url)
            ]);

            return {
                success: true,
                filename: filename,
                headerBytes: headerBytes || [],
                textContent: textContent || ''
            };

        } catch (error) {
            console.warn('fileScanner: fetchScanData failed —', error);
            return {
                success: false,
                reason: 'error',
                headerBytes: [],
                textContent: ''
            };
        }
    }

    // TODO: AI integration hook — stream scan progress to AI commander
    // TODO: WebGL/GPU offload — pass large byte arrays to GPU compute shader
    // TODO: Worker thread support for non-blocking large file scans
};