using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nzbhydra_schedule
{
    internal class NzbHydra
    {
        public DirectoryInfo NzbDirectory { get; set; }
        public FileInfo SearchTermFilePath { get; set; }
        public List<string> SearchTerms { get; private set; }
        private string? ApiKey { get; set; }
        public bool AppendResolution { get; set; } = false;
        public string? DefaultResolution { get; set; } = "1080p";
        public int RequestCooldown { get; set; } = 10;
        public string? HydraUrl { get; set; }
        public int MinSize { get; set; }
        public int MaxSize { get; set; }

        public NzbHydra(DirectoryInfo nzbDirectory, FileInfo searchTermFilePath, string? apiKey, string? hydraUrl) : this(nzbDirectory, searchTermFilePath, apiKey, hydraUrl, false, "1080p", 10, 1, 900000) {
        }


        public NzbHydra(DirectoryInfo nzbDirectory, FileInfo searchTermFilePath, string? apiKey, string? hydraUrl, bool appendResolution, string? defaultResolution, int requestCooldown, int minSize, int maxSize)
        {
            NzbDirectory = nzbDirectory;
            SearchTermFilePath = searchTermFilePath;
            ApiKey = apiKey;
            AppendResolution = appendResolution;
            DefaultResolution = defaultResolution;
            RequestCooldown = requestCooldown;
            HydraUrl = hydraUrl;
            MinSize = minSize;
            MaxSize = maxSize;
        }

        private async Task ReadSearchTermFileAsync()
        {
            SearchTerms = (await File.ReadAllLinesAsync(SearchTermFilePath.FullName, Encoding.UTF8))
                .Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("#"))
                .Distinct()
                .ToList();
        }

        private async Task VerifyParams()
        {
            if (!SearchTermFilePath.Exists)
            {
                throw new ArgumentException($"Search Term File does not exist at {SearchTermFilePath.FullName}.");
            }

            if(!NzbDirectory.Exists)
            {
                throw new ArgumentException($"The nzb directory {NzbDirectory.FullName} does not exist.");
            }
        }

        private async Task VerifyHydraConnection()
        {

        }

        public async Task GetNzbsAsync()
        {
            await VerifyParams();
            await VerifyHydraConnection();


        }
    }
}
