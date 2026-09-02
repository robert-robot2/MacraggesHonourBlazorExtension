namespace MacraggesHonourBlazor
{
    public class FileScannerService
    {
        // =============================================
        // SUB SCANNERS
        // =============================================
        private readonly FileExtensionScanner _extensionScanner = new();
        private readonly FileWordPatternScanner _wordScanner = new();
        private readonly FileCodePatternScanner _codeScanner = new();

        // =============================================
        // MAIN SCAN CONTROLLER
        // =============================================
        public async Task<FileScanReport> ScanAsync(string filename, byte[] headerBytes, string textContent)
        {
            var report = new FileScanReport
            {
                Filename = filename,
                ScannedAt = DateTime.Now.ToString()
            };

            // Run all scanners
            var extensionResult = _extensionScanner.Scan(filename, headerBytes);
            var wordResult = _wordScanner.Scan(filename, textContent);
            var codeResult = _codeScanner.Scan(filename, textContent);

            report.Results = new List<ScanResult> { extensionResult, wordResult, codeResult };

            // Aggregate final verdict — worst result wins
            report.FinalStatus = AggregateSatus(extensionResult, wordResult, codeResult);
            report.FinalMessage = BuildFinalMessage(report.FinalStatus, report.Results, report.Filename);

            // TODO: AI integration hook — pass report to AI commander
            // TODO: Feed report into AI natural language summary
            // TODO: Trigger WebGL skull animation based on threat level

            return await Task.FromResult(report);
        }

        // =============================================
        // AGGREGATE — WORST RESULT WINS
        // =============================================
        private string AggregateSatus(params ScanResult[] results)
        {
            if (results.Any(r => r.Status == "threat")) return "threat";
            if (results.Any(r => r.Status == "suspicious")) return "suspicious";
            if (results.Any(r => r.Status == "clean")) return "clean";
            return "unknown";
        }

        // =============================================
        // BUILD FINAL MESSAGE
        // =============================================
        private string BuildFinalMessage(string status, List<ScanResult> results, string filename)
        {
            var activeResults = results
                .Where(r => r.Status != "skipped" && r.Status != "unknown")
                .ToList();

            var summary = $"File: {filename} | " + string.Join(" | ", activeResults.Select(r => r.Message));

            return status switch
            {
                "threat" => $"🔴 THREAT DETECTED — {summary}",
                "suspicious" => $"🟡 SUSPICIOUS FILE — {summary}",
                "clean" => $"🟢 FILE APPEARS CLEAN — {summary}",
                _ => $"⚪ SCAN INCONCLUSIVE — {summary}"
            };
        }
    }

    // =============================================
    // SHARED MODELS
    // =============================================
    public class ScanResult
    {
        public string Status { get; set; } = "unknown";
        public string Message { get; set; } = "";
        public string Scanner { get; set; } = "";
    }

    public class FileScanReport
    {
        public string Filename { get; set; } = "";
        public string ScannedAt { get; set; } = "";
        public string FinalStatus { get; set; } = "unknown";
        public string FinalMessage { get; set; } = "";
        public List<ScanResult> Results { get; set; } = new();
    }
}