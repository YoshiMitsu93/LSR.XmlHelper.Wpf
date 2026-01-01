using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LSR.XmlHelper.Wpf.Services.Updates
{
    public sealed class GitHubReleaseService
    {
        private readonly HttpClient _http;

        public GitHubReleaseService(HttpClient http)
        {
            _http = http;
            if (!_http.DefaultRequestHeaders.UserAgent.TryParseAdd("LSR-XmlHelper-Wpf"))
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("LSR-XmlHelper-Wpf");
        }

        public async Task<GitHubReleaseInfo?> GetLatestReleaseAsync(string owner, string repo)
        {
            var url = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            using var response = await _http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var root = doc.RootElement;

            var tagName = root.TryGetProperty("tag_name", out var tagProp) ? tagProp.GetString() : null;
            var htmlUrl = root.TryGetProperty("html_url", out var urlProp) ? urlProp.GetString() : null;

            Version? version = null;
            if (!string.IsNullOrWhiteSpace(tagName))
            {
                var cleaned = tagName.Trim();
                if (cleaned.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                    cleaned = cleaned.Substring(1);

                if (Version.TryParse(cleaned, out var parsed))
                    version = parsed;
            }

            return new GitHubReleaseInfo(tagName ?? "", htmlUrl ?? "", version);
        }
    }

    public sealed class GitHubReleaseInfo
    {
        public GitHubReleaseInfo(string tagName, string htmlUrl, Version? version)
        {
            TagName = tagName;
            HtmlUrl = htmlUrl;
            Version = version;
        }

        public string TagName { get; }
        public string HtmlUrl { get; }
        public Version? Version { get; }
    }
}
