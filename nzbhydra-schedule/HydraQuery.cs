using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nzbhydra_schedule
{
    internal class HydraQuery
    {
        public string SearchTerm { get; set; }
        public int StatusCode { get; set; }
        public List<HydraResult> Results { get; set; }

        public HydraQuery(string searchTerm) {
            SearchTerm = searchTerm;
            Results = new List<HydraResult>();
        }

        public void ParseResults(string results)
        {
            Logger.WriteLog($"Parsing result xml searchterm {SearchTerm}", Logger.LogLevel.debug);
            var xDoc = XDocument.Parse(results);
            var resultItems = xDoc.Descendants("item");

            Results = resultItems
                .Select(x => new HydraResult(x))
                .ToList();
            Logger.WriteLog($"Parsed {Results.Count} results.", Logger.LogLevel.debug);
        }

        private string? GetNzbFilename(HttpResponseMessage linkResponse)
        {

            var localPath = linkResponse.RequestMessage.RequestUri.LocalPath;
            var pathArray = localPath.Split('/');
            return pathArray[pathArray.Length - 1];
        }

        public async Task SaveNzbAsync(HttpClient httpClient, DirectoryInfo directory)
        {
            foreach (var result in Results)
            {
                Logger.WriteLog($"Downloading from link {result.Link}.", Logger.LogLevel.debug);
                HttpResponseMessage linkResponse;

                try
                {
                    linkResponse = await httpClient.GetAsync(result.Link);
                }
                catch (Exception ex)
                {
                    Logger.WriteLog($"An error occurred when downloading link {result.Link} for result {result.Title}: {ex.Message}", Logger.LogLevel.err);
                    continue;
                }

                var nzbFilename = GetNzbFilename(linkResponse);
                if (nzbFilename == null )
                {
                    Logger.WriteLog($"Could not get filename from download.", Logger.LogLevel.err);
                    continue;
                }

                var nzbPath = Path.Join(directory.FullName, nzbFilename);
                Logger.WriteLog($"Downloading nzb to {nzbPath}.");

                if (File.Exists(nzbPath))
                {
                    Logger.WriteLog($"{nzbPath} already exists, skpping download", Logger.LogLevel.warn);
                    continue;
                }

                using var fileStream = File.Create(nzbPath);
                await linkResponse.Content.CopyToAsync(fileStream);

                if (!File.Exists(nzbPath))
                {
                    Logger.WriteLog($"An error occurred when saving the downloaded content to {nzbPath}.", Logger.LogLevel.err);
                    continue;
                }
            }
        }
        }
}
