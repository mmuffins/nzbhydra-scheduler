using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nzbhydra_schedule
{
    internal class NzbHydra
    {
        public DirectoryInfo NzbDirectory { get; set; }
        public FileInfo SearchTermFilePath { get; set; }
        public List<string> SearchTerms { get; private set; }
        public string ApiKey { get; set; }
        public bool AppendResolution { get; set; }
        public string DefaultResolution { get; set; }
        public int RequestCooldown { get; set; }
        public string HydraUrl { get; set; }
        public int MinSize { get; set; }
        public int MaxSize { get; set; }
        public int MaxAge { get; set; }
        public string Category { get; set; }
        public string Indexers { get; set; }

        public NzbHydra() { }

        public NzbHydra(DirectoryInfo nzbDirectory, FileInfo searchTermFilePath, string apiKey, string hydraUrl, int maxAge, string category, string indexers) : this(nzbDirectory, searchTermFilePath, apiKey, hydraUrl, maxAge, category, indexers, false, "1080p", 10, 1, 900000) {
        }


        public NzbHydra(DirectoryInfo nzbDirectory, FileInfo searchTermFilePath, string apiKey, string hydraUrl, int maxAge, string category, string indexers, bool appendResolution, string? defaultResolution, int requestCooldown, int minSize, int maxSize)
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
            MaxAge = maxAge;
            Category = category;
            Indexers = indexers;
        }

        private async Task ReadSearchTermFileAsync()
        {
            Logger.WriteLog($"Reading search term file {SearchTermFilePath.FullName}.", Logger.LogLevel.debug);
            
            if (!SearchTermFilePath.Exists)
            {
                Logger.Throw(new ArgumentException($"Could not find search term file at {SearchTermFilePath.FullName}."));
            }

            try
            {
                SearchTerms = (await File.ReadAllLinesAsync(SearchTermFilePath.FullName, Encoding.UTF8))
                    .Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("#"))
                    .Select(str => str.Trim())
                    .Distinct()
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.Throw(ex);
            }
        }

        private async Task VerifyHydraConnection()
        {

        }

        public async Task GetNzbsAsync()
        {
            if (!NzbDirectory.Exists)
            {
                Logger.Throw(new ArgumentException($"Could not find nzb directory {NzbDirectory.FullName}."));
            }

            await VerifyHydraConnection();

            await ReadSearchTermFileAsync();

            Logger.WriteLog($"Initializing http client.", Logger.LogLevel.debug);
            using var httpClient = new HttpClient();
            foreach (var item in SearchTerms)
            {
                var searchTerm = item;
                if (AppendResolution)
                {
                    searchTerm += $" {DefaultResolution}";
                }

                Logger.WriteLog($"Checking search term {searchTerm}.");

                await QueryNzbHydra(httpClient, searchTerm);
            }
        }

        private async Task QueryNzbHydra(HttpClient httpClient,string searchTerm)
        {
            var uri = $"http://{HydraUrl}/api?apikey={ApiKey}&t=search&extended=1&password=1&limit=100&offset=0&category={Category}& indexers={Indexers}&minsize={MinSize}&maxsize={MaxSize}&maxage={MaxAge}&q={searchTerm}";
            Logger.WriteLog($"Requesting {uri}", Logger.LogLevel.debug);

            string? content = null;
            //try
            //{
            //    var response = await httpClient.GetAsync(uri);
            //    content = await response.Content.ReadAsStringAsync();
            //}
            //catch (Exception ex)
            //{
            //    Logger.Throw(ex);
            //}

            //var xDoc = XDocument.Parse(content);

            //var results = xDoc.Descendants("item");

        }
    }
}
