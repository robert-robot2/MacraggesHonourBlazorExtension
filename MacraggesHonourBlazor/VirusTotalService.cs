using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MacraggesHonourBlazor
{
    public class VirusTotalService
    {
        private readonly HttpClient _http;

        public VirusTotalService()
        {
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Add("x-apikey", Config.VirusTotalApiKey);
        }

        public async Task<VtResult> ScanUrl(string url)
        {
            try
            {
                if (url.StartsWith("blob:"))
                    return new VtResult { Status = "blob", Message = "⚪ Email attachment — cannot scan" };

                // Step 1 - Submit URL
                var body = new StringContent($"url={Uri.EscapeDataString(url)}", Encoding.UTF8, "application/x-www-form-urlencoded");
                var submitResponse = await _http.PostAsync("https://www.virustotal.com/api/v3/urls", body);

                if (!submitResponse.IsSuccessStatusCode)
                    return new VtResult { Status = "error", Message = "⚠️ VirusTotal API error" };

                var submitData = await submitResponse.Content.ReadFromJsonAsync<JsonElement>();
                var analysisId = submitData.GetProperty("data").GetProperty("id").GetString();

                // Step 2 - Get results
                var resultResponse = await _http.GetAsync($"https://www.virustotal.com/api/v3/analyses/{analysisId}");

                if (!resultResponse.IsSuccessStatusCode)
                    return new VtResult { Status = "error", Message = "⚠️ Could not retrieve results" };

                var resultData = await resultResponse.Content.ReadFromJsonAsync<JsonElement>();
                var stats = resultData.GetProperty("data").GetProperty("attributes").GetProperty("stats");

                var malicious = stats.GetProperty("malicious").GetInt32();
                var suspicious = stats.GetProperty("suspicious").GetInt32();
                var harmless = stats.GetProperty("harmless").GetInt32();
                var undetected = stats.GetProperty("undetected").GetInt32();
                var total = malicious + suspicious + harmless + undetected;

                if (malicious > 0)
                    return new VtResult { Status = "threat", Message = $"🔴 THREAT DETECTED: {malicious}/{total} engines flagged" };
                else if (suspicious > 0)
                    return new VtResult { Status = "suspicious", Message = $"🟡 SUSPICIOUS: {suspicious}/{total} engines flagged" };
                else if (total > 0)
                    return new VtResult { Status = "clean", Message = $"🟢 CLEAN: 0/{total} engines flagged" };
                else
                    return new VtResult { Status = "unknown", Message = "⚪ UNKNOWN: Not in database yet" };
            }
            catch (Exception)
            {
                return new VtResult { Status = "error", Message = "⚠️ Scan failed — network error" };
            }
        }
    }

    public class VtResult
    {
        public string Status { get; set; } = "";
        public string Message { get; set; } = "";
    }
}