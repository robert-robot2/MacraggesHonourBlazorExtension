using System.Collections.Generic;

namespace MacraggesHonourBlazor
{
    public class FileExtensionScanner
    {
        // =============================================
        // KNOWN EXTENSION RISK LEVELS
        // =============================================
        private static readonly Dictionary<string, string> ExtensionRisk = new()
        {
            // High risk
            { ".exe",  "threat" },
            { ".bat",  "threat" },
            { ".cmd",  "threat" },
            { ".ps1",  "threat" },
            { ".vbs",  "threat" },
            { ".msi",  "threat" },
            { ".dll",  "threat" },
            { ".scr",  "threat" },
            { ".com",  "threat" },

            // Suspicious
            { ".js",   "suspicious" },
            { ".jar",  "suspicious" },
            { ".zip",  "suspicious" },
            { ".rar",  "suspicious" },
            { ".7z",   "suspicious" },
            { ".iso",  "suspicious" },
            { ".lnk",  "suspicious" },
            { ".pdf",  "suspicious" },
            { ".doc",  "suspicious" },
            { ".docx", "suspicious" },
            { ".xls",  "suspicious" },
            { ".xlsx", "suspicious" },

            // Clean
            { ".png",  "clean" },
            { ".jpg",  "clean" },
            { ".jpeg", "clean" },
            { ".gif",  "clean" },
            { ".fbx",  "clean" },
            { ".obj",  "clean" },
            { ".stl",  "clean" },
            { ".mp3",  "clean" },
            { ".mp4",  "clean" },
            { ".txt",  "clean" },
        };

        // =============================================
        // MAGIC NUMBER BYTE SIGNATURES
        // =============================================
        private static readonly Dictionary<string, byte[]> MagicNumbers = new()
        {
            { "PE_EXECUTABLE", new byte[] { 0x4D, 0x5A } },           // MZ header — Windows EXE/DLL
            { "PDF",           new byte[] { 0x25, 0x50, 0x44, 0x46 }}, // %PDF
            { "ZIP",           new byte[] { 0x50, 0x4B, 0x03, 0x04 }}, // PK zip
            { "PNG",           new byte[] { 0x89, 0x50, 0x4E, 0x47 }}, // PNG
            { "JPEG",          new byte[] { 0xFF, 0xD8, 0xFF } },       // JPEG
            { "RAR",           new byte[] { 0x52, 0x61, 0x72, 0x21 }}, // Rar!
            { "ISO",           new byte[] { 0x43, 0x44, 0x30, 0x30 }}, // CD001
        };

        // =============================================
        // SCAN
        // =============================================
        public ScanResult Scan(string filename, byte[] headerBytes)
        {
            var ext = Path.GetExtension(filename).ToLower();

            // Check extension risk
            var extStatus = ExtensionRisk.TryGetValue(ext, out var risk) ? risk : "unknown";

            // Check magic numbers — does the file header match the extension?
            var magicMismatch = CheckMagicMismatch(ext, headerBytes);

            if (magicMismatch)
            {
                return new ScanResult
                {
                    Status = "threat",
                    Message = $"🔴 MAGIC NUMBER MISMATCH — {ext} file has wrong header bytes. Possible disguised executable!",
                    Scanner = "ExtensionScanner"
                };
            }

            // PE header in a non-exe file is always a red flag
            if (ext != ".exe" && ext != ".dll" && ext != ".scr" && HasPeHeader(headerBytes))
            {
                return new ScanResult
                {
                    Status = "threat",
                    Message = $"🔴 PE HEADER DETECTED in {ext} file — executable disguised as {ext}!",
                    Scanner = "ExtensionScanner"
                };
            }

            return new ScanResult
            {
                Status = "suspicious",
                Message = extStatus switch
                {
                    "threat" => $"🔴 HIGH RISK file type detected: {ext}",
                    "suspicious" => $"🟡 SUSPICIOUS file type detected: {ext}",
                    "clean" => $"🟡 File type detected: {ext}",
                    _ => $"🟡 File type detected: {ext}"
                },
                Scanner = "ExtensionScanner"
            };
        }

        private bool HasPeHeader(byte[] bytes)
        {
            if (bytes.Length < 2) return false;
            return bytes[0] == 0x4D && bytes[1] == 0x5A;
        }

        private bool CheckMagicMismatch(string ext, byte[] bytes)
        {
            if (bytes.Length < 4) return false;

            // PNG extension should have PNG magic bytes
            if (ext == ".png" && !StartsWith(bytes, MagicNumbers["PNG"])) return true;
            if (ext == ".jpg" || ext == ".jpeg")
                if (!StartsWith(bytes, MagicNumbers["JPEG"])) return true;
            if (ext == ".pdf" && !StartsWith(bytes, MagicNumbers["PDF"])) return true;
            if (ext == ".zip" && !StartsWith(bytes, MagicNumbers["ZIP"])) return true;
            if (ext == ".rar" && !StartsWith(bytes, MagicNumbers["RAR"])) return true;

            return false;
        }

        private bool StartsWith(byte[] data, byte[] pattern)
        {
            if (data.Length < pattern.Length) return false;
            for (int i = 0; i < pattern.Length; i++)
                if (data[i] != pattern[i]) return false;
            return true;
        }

        // TODO: AI integration hook — pass flagged results to AI commander for natural language reporting
        // TODO: Expand MagicNumbers dictionary with more signatures
    }
}