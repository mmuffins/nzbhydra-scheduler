using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nzbhydra_schedule
{
    internal class NzbHydra
    {
        public DirectoryInfo NzbDirectory { get; set; }
        public FileInfo SearchTermFilePath { get; set; }
        public List<HydraQuery> Searches { get; private set; }
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

        public NzbHydra(DirectoryInfo nzbDirectory, FileInfo searchTermFilePath, string apiKey, string hydraUrl, int maxAge, string category, string indexers) : this(nzbDirectory, searchTermFilePath, apiKey, hydraUrl, maxAge, category, indexers, false, "1080p", 10, 1, 900000)
        {
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
                Searches = (await File.ReadAllLinesAsync(SearchTermFilePath.FullName, Encoding.UTF8))
                    .Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("#"))
                    .Select(str => str.Trim())
                    .Distinct()
                    .Select(x => new HydraQuery(AppendResolution ? $"{x} {DefaultResolution}" : x))
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.Throw(ex);
            }
        }

        private async Task<List<string>> ReadFileAsync(FileInfo path)
        {
            Logger.WriteLog($"Reading file {path.FullName}.", Logger.LogLevel.debug);
            try
            {
                return (await File.ReadAllLinesAsync(path.FullName, Encoding.UTF8))
                    .Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("#"))
                    .Select(str => str.Trim())
                    .Distinct()
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.Throw(ex);
            }

            return new List<string>();
        }

        public async Task GenerateSearchTerms(FileInfo groupsFile, FileInfo showsFile)
        {
            if (!groupsFile.Exists)
            {
                Logger.Throw(new ArgumentException($"Could not find groups file at {groupsFile.FullName}."));
            }

            if (!showsFile.Exists)
            {
                Logger.Throw(new ArgumentException($"Could not find shows file at {showsFile.FullName}."));
            }

            if (!SearchTermFilePath.Directory.Exists)
            {
                Logger.Throw(new ArgumentException($"Could not output file folder {SearchTermFilePath.Directory.FullName}."));
            }


            List<string> groups = await ReadFileAsync(groupsFile);
            Logger.WriteLog($"Found {groups.Count} in groups file.");

            if(groups.Count == 0)
            {
                Logger.Throw(new ArgumentOutOfRangeException($"No entries found in groups file, aborting program."));
            }

            List<string> shows = await ReadFileAsync(showsFile);
            Logger.WriteLog($"Found {shows.Count} in shows file.");
            if (shows.Count == 0)
            {
                Logger.Throw(new ArgumentOutOfRangeException($"No entries found in shows file, aborting program."));
            }

            Logger.WriteLog($"Initializing http client.", Logger.LogLevel.debug);

            try
            {
                using (StreamWriter sw = File.AppendText(SearchTermFilePath.FullName))
                await sw.WriteLineAsync(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            }
            catch (Exception ex)
            {
                Logger.Throw(new Exception($"An error occurred when writing to file {SearchTermFilePath.FullName}: {ex.Message}"));
            }

            using var httpClient = new HttpClient();
            foreach (var query in shows)
            {
                Logger.WriteLog($"Checking show {query}.");
                List<string> matchingGroups = await FindBestGroup(httpClient, query, groups);
                Logger.WriteLog($"Validating results.");
                var validatedSearchTerm = await ValidateGroupSearchTerms(httpClient, matchingGroups, query);

                try
                {
                    using (StreamWriter sw = File.AppendText(SearchTermFilePath.FullName))
                    await sw.WriteLineAsync(validatedSearchTerm);
                }
                catch (Exception ex)
                {
                    Logger.Throw(new Exception($"An error occurred when writing to file {SearchTermFilePath.FullName}: {ex.Message}"));
                }
                
                System.Threading.Thread.Sleep(RequestCooldown * 1000);
            }
        }

        private async Task<List<string>> FindBestGroup(HttpClient httpClient, string show, List<string> groups)
        {
            var searchTerm = Regex.Replace(show, @":|,|\(|\)", "");
            var query = new HydraQuery(searchTerm);
            await QueryNzbHydra(httpClient, query);
            var validGroups = new List<string>();

            if (query.Results.Count == 0)
            {
                Logger.WriteLog("Could not find results for search term {searchTerm}", Logger.LogLevel.err);
                return validGroups;
            }

            // shuffle groups to not always get the same one on top
            var rnd = new Random();
            var shuffledGroups = groups.OrderBy(item => rnd.Next());

            foreach (var group in shuffledGroups)
            {
                // check which of the show search results contains the group and resolution
                Logger.WriteLog($"Evaluating group {group} and resolution {DefaultResolution}", Logger.LogLevel.debug);
                var loopResult = query.Results
                    .Where(r => r.Title.Contains(group, StringComparison.InvariantCultureIgnoreCase))
                    .Where(r => r.Title.Contains(DefaultResolution, StringComparison.InvariantCultureIgnoreCase))
                    .Select(r => r.Title)
                    .ToList();

                Logger.WriteLog($"Found {loopResult.Count} results for group {group}.");
                loopResult.ForEach(r =>
                {
                    Logger.WriteLog($"Result: {r}", Logger.LogLevel.debug);
                });

                validGroups.Add(group);
            }
            return validGroups;
        }

        private async Task<string> ValidateGroupSearchTerms(HttpClient httpClient, List<string> groups, string searchTerm)
        {
            foreach (var group in groups)
            {
                var validateSearchTerm = $"{group} {Regex.Replace(searchTerm, @":|,|\(|\)", "")} {DefaultResolution}";
                Logger.WriteLog($"Validating '{validateSearchTerm}'.");

                var query = new HydraQuery(validateSearchTerm);
                await QueryNzbHydra(httpClient, query);

                if (query.Results.Count == 0)
                {
                    Logger.WriteLog($"Could not validate search term '{validateSearchTerm}'.");
                    continue;
                }
                Logger.WriteLog($"Successfully validated search term '{validateSearchTerm}'.");
                return validateSearchTerm;
            }
            Logger.WriteLog($"Could not validate any of the provided search terms.", Logger.LogLevel.warn);
            return string.Empty;
        }

        public async Task StartSearchAsync()
        {
            if (!NzbDirectory.Exists)
            {
                Logger.Throw(new ArgumentException($"Could not find output directory {NzbDirectory.Exists}."));
            }

            await ReadSearchTermFileAsync();

            Logger.WriteLog($"Initializing http client.", Logger.LogLevel.debug);
            using var httpClient = new HttpClient();
            foreach (var query in Searches)
            {
                Logger.WriteLog($"Checking search term {query.SearchTerm}.");
                await QueryNzbHydra(httpClient, query);
                await query.SaveNzbAsync(httpClient, NzbDirectory);
                System.Threading.Thread.Sleep(RequestCooldown * 1000);
            }

        }

        private async Task QueryNzbHydra(HttpClient httpClient, HydraQuery query)
        {
            var uri = $"http://{HydraUrl}/api?apikey={ApiKey}&t=search&extended=1&password=1&limit=100&offset=0&category={Category}&indexers={Indexers}&minsize={MinSize}&maxsize={MaxSize}&maxage={MaxAge}&q={query.SearchTerm}";
            Logger.WriteLog($"Requesting {uri}", Logger.LogLevel.debug);

            string? content = null;
            try
            {
                var response = await httpClient.GetAsync(uri);
                query.StatusCode = (int)response.StatusCode;
                Logger.WriteLog($"Response: {query.StatusCode}", Logger.LogLevel.debug); ;

                content = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"An error occurred: {ex.Message}", Logger.LogLevel.err);
                return;
            }

            query.ParseResults(content);
            if (query.Results.Count == 0)
            {
                Logger.WriteLog($"Could not find results for {query.SearchTerm}", Logger.LogLevel.info);
                //searchTerm += " NORESULT";
                //using (var writer = File.AppendText(errorPath))
                //{
                //    writer.WriteLine(query);
                //}
                return;
            }
            Logger.WriteLog($"Found {query.Results.Count} results");

        }
    }
}
    