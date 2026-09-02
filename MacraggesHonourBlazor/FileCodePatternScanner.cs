namespace MacraggesHonourBlazor
{
    public class FileCodePatternScanner
    {
        // =============================================
        // EXTENSIONS THIS SCANNER HANDLES
        // =============================================
        private static readonly HashSet<string> CodeExtensions = new()
        {
            ".js", ".ts", ".py", ".cs", ".cpp", ".c", ".h",
            ".java", ".php", ".rb", ".sh", ".ps1", ".bat",
            ".cmd", ".vbs", ".html", ".htm", ".xml", ".json"
        };

        // =============================================
        // HIGH RISK PATTERNS — LIKELY MALICIOUS
        // =============================================
        private static readonly List<(string Pattern, string Reason)> HighRiskPatterns = new()
        {
            // Malware naming conventions
            ("malware",          "Malware keyword detected"),
            ("ransomware",       "Ransomware keyword detected"),
            ("keylogger",        "Keylogger keyword detected"),
            ("rootkit",          "Rootkit keyword detected"),
            ("backdoor",         "Backdoor keyword detected"),
            ("trojan",           "Trojan keyword detected"),
            ("exploit",          "Exploit keyword detected"),
            ("payload",          "Payload keyword detected"),

            // Dangerous API calls
            ("shell.exec",       "Shell execution detected"),
            ("shell_exec",       "Shell execution detected"),
            ("exec(",            "Code execution detected"),
            ("eval(",            "Eval execution detected"),
            ("system(",          "System call detected"),
            ("passthru(",        "Passthru call detected"),
            ("proc_open(",       "Process open detected"),
            ("popen(",           "Process open detected"),

            // Network exfiltration patterns
            ("exfiltrat",        "Data exfiltration keyword detected"),
            ("C2",               "Command and control reference detected"),
            ("c2server",         "C2 server reference detected"),
            ("botnet",           "Botnet keyword detected"),

            // Crypto/ransomware patterns
            ("encrypt(",         "Encryption function detected"),
            ("AES.encrypt",      "AES encryption detected"),
            ("ransom",           "Ransom keyword detected"),
            (".locky",           "Locky ransomware extension detected"),
            (".wannacry",        "WannaCry reference detected"),

            // Registry manipulation
            ("RegCreateKey",     "Registry manipulation detected"),
            ("RegSetValue",      "Registry write detected"),
            ("HKEY_LOCAL_MACHINE","Registry access detected"),

            // Privilege escalation
            ("privilege",        "Privilege reference detected"),
            ("escalat",          "Privilege escalation keyword detected"),
            ("runas",            "RunAs execution detected"),
            ("sudo",             "Sudo execution detected"),
        };

        // =============================================
        // SUSPICIOUS PATTERNS — CONTEXT DEPENDENT
        // =============================================
        private static readonly List<(string Pattern, string Reason)> SuspiciousPatterns = new()
        {
            // Obfuscation techniques
            ("fromCharCode",     "Character code obfuscation"),
            ("atob(",            "Base64 decode detected"),
            ("btoa(",            "Base64 encode detected"),
            ("unescape(",        "Unescape obfuscation detected"),
            ("String.fromChar",  "String obfuscation detected"),

            // Python specific — no syntax so word patterns matter more
            ("#include",         "C include directive detected"),
            ("import os",        "OS module import detected"),
            ("import sys",       "Sys module import detected"),
            ("subprocess",       "Subprocess module detected"),
            ("__import__",       "Dynamic import detected"),
            ("os.system",        "OS system call detected"),
            ("os.popen",         "OS popen detected"),

            // Network patterns
            ("socket(",          "Socket connection detected"),
            ("bind(",            "Socket bind detected"),
            ("connect(",         "Socket connect detected"),
            ("urllib",           "URL lib import detected"),
            ("requests.get",     "HTTP request detected"),
            ("requests.post",    "HTTP POST request detected"),

            // File system manipulation
            ("deleteFile",       "File deletion detected"),
            ("fs.unlink",        "File deletion detected"),
            ("os.remove",        "File removal detected"),
            ("shutil.rmtree",    "Directory removal detected"),
            ("format(",          "Drive format reference detected"),
        };

        // =============================================
        // WHITELIST — REDUCE FALSE POSITIVES
        // =============================================
        private static readonly HashSet<string> WhitelistedContexts = new()
        {
            "test", "example", "demo", "sample", "tutorial",
            "documentation", "readme", "comment", "placeholder"
        };

        // =============================================
        // SCAN
        // =============================================
        public ScanResult Scan(string filename, string textContent)
        {
            var ext = Path.GetExtension(filename).ToLower();

            if (!CodeExtensions.Contains(ext))
            {
                return new ScanResult
                {
                    Status = "skipped",
                    Message = $"⚪ Code pattern scan skipped for {ext}",
                    Scanner = "CodePatternScanner"
                };
            }

            if (string.IsNullOrWhiteSpace(textContent))
            {
                return new ScanResult
                {
                    Status = "unknown",
                    Message = "⚪ No content to scan",
                    Scanner = "CodePatternScanner"
                };
            }

            var highRiskFindings = new List<string>();
            var suspiciousFindings = new List<string>();
            var lowerContent = textContent.ToLower();

            // Check whitelist context
            bool isWhitelisted = WhitelistedContexts.Any(w => lowerContent.Contains(w));

            // Scan high risk patterns
            foreach (var (pattern, reason) in HighRiskPatterns)
            {
                if (textContent.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    highRiskFindings.Add(reason);
            }

            // Scan suspicious patterns
            foreach (var (pattern, reason) in SuspiciousPatterns)
            {
                if (textContent.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    suspiciousFindings.Add(reason);
            }

            // High risk findings always flag regardless of whitelist
            if (highRiskFindings.Count >= 1)
            {
                return new ScanResult
                {
                    Status = "threat",
                    Message = $"🔴 MALICIOUS CODE PATTERNS DETECTED — {highRiskFindings.Count} findings: {string.Join(", ", highRiskFindings.Take(3))}",
                    Scanner = "CodePatternScanner"
                };
            }

            // Suspicious findings downgraded if whitelisted
            if (suspiciousFindings.Count >= 3 && !isWhitelisted)
            {
                return new ScanResult
                {
                    Status = "threat",
                    Message = $"🔴 MULTIPLE SUSPICIOUS PATTERNS — {suspiciousFindings.Count} findings: {string.Join(", ", suspiciousFindings.Take(3))}",
                    Scanner = "CodePatternScanner"
                };
            }
            else if (suspiciousFindings.Count >= 1 && !isWhitelisted)
            {
                return new ScanResult
                {
                    Status = "suspicious",
                    Message = $"🟡 SUSPICIOUS CODE PATTERNS — {suspiciousFindings[0]}",
                    Scanner = "CodePatternScanner"
                };
            }

            return new ScanResult
            {
                Status = "clean",
                Message = "🟢 No malicious code patterns detected",
                Scanner = "CodePatternScanner"
            };

            // TODO: AI integration hook — feed pattern matches to AI commander for threat assessment
            // TODO: WebGL/GPU offload for large file scanning
            // TODO: Add YARA-style rule engine for advanced pattern matching
            // TODO: Expand with real malware signature database
        }
    }
}