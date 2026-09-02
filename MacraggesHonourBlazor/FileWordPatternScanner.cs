namespace MacraggesHonourBlazor
{
    public class FileWordPatternScanner
    {
        // =============================================
        // EXTENSIONS THIS SCANNER HANDLES
        // =============================================
        private static readonly HashSet<string> TextExtensions = new()
        {
            ".txt", ".csv", ".log", ".md", ".rtf", ".xml", ".json", ".yaml", ".yml"
        };

        // =============================================
        // SUSPICIOUS PATTERNS IN PLAIN TEXT DOCS
        // =============================================
        private static readonly List<(string Pattern, string Reason)> SuspiciousPatterns = new()
        {
            // Code syntax in plain text docs is suspicious
            ("()", "Function call syntax detected"),
            ("{}", "Code block syntax detected"),
            ("};", "Code block closure detected"),
            ("=>", "Lambda/arrow function detected"),
            ("#!/", "Shebang line detected"),
            ("%!PS", "PostScript code detected"),

            // Math/formula patterns that could mask code
            ("=0;", "Assignment with terminator detected"),
            ("++", "Increment operator detected"),
            ("--", "Decrement operator detected"),

            // Encoding patterns often used in obfuscation
            ("base64", "Base64 encoding reference detected"),
            ("fromCharCode", "Character code obfuscation detected"),
            ("\\x", "Hex escape sequence detected"),
            ("\\u00", "Unicode escape sequence detected"),

            // Network patterns in plain text
            ("http://", "URL in plain text document"),
            ("https://", "URL in plain text document"),
            ("ftp://", "FTP reference detected"),

            // Registry/system references
            ("HKEY_", "Windows registry reference detected"),
            ("System32", "System32 reference detected"),
            ("%APPDATA%", "AppData reference detected"),
            ("%TEMP%", "Temp directory reference detected"),
        };

        // =============================================
        // CLEAN PATTERNS — REDUCE FALSE POSITIVES
        // =============================================
        private static readonly List<string> CleanContexts = new()
        {
            // Math equations in educational docs are fine
            "math equation",
            "formula",
            "calculation",
            "example",
        };

        // =============================================
        // SCAN
        // =============================================
        public ScanResult Scan(string filename, string textContent)
        {
            var ext = Path.GetExtension(filename).ToLower();

            // Only scan text-type files
            if (!TextExtensions.Contains(ext))
            {
                return new ScanResult
                {
                    Status = "skipped",
                    Message = $"⚪ Word pattern scan skipped for {ext}",
                    Scanner = "WordPatternScanner"
                };
            }

            if (string.IsNullOrWhiteSpace(textContent))
            {
                return new ScanResult
                {
                    Status = "unknown",
                    Message = "⚪ No text content to scan",
                    Scanner = "WordPatternScanner"
                };
            }

            var findings = new List<string>();
            var lowerContent = textContent.ToLower();

            // Check for clean contexts first to reduce false positives
            bool hasCleanContext = CleanContexts.Any(c => lowerContent.Contains(c));

            foreach (var (pattern, reason) in SuspiciousPatterns)
            {
                if (textContent.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    // If clean context found downgrade severity
                    if (!hasCleanContext)
                        findings.Add(reason);
                }
            }

            // Score based findings
            if (findings.Count >= 3)
            {
                return new ScanResult
                {
                    Status = "threat",
                    Message = $"🔴 MULTIPLE ANOMALIES in text document — {findings.Count} suspicious patterns: {string.Join(", ", findings.Take(3))}",
                    Scanner = "WordPatternScanner"
                };
            }
            else if (findings.Count >= 1)
            {
                return new ScanResult
                {
                    Status = "suspicious",
                    Message = $"🟡 SUSPICIOUS content in text document — {findings[0]}",
                    Scanner = "WordPatternScanner"
                };
            }

            return new ScanResult
            {
                Status = "clean",
                Message = "🟢 No suspicious patterns found in text document",
                Scanner = "WordPatternScanner"
            };
        }

        // TODO: AI integration hook — feed findings to AI for contextual analysis
        // TODO: Expand pattern list with more obfuscation techniques
        // TODO: WebGL/GPU offload for large document scanning
    }
}