using System;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace nzbhydra_schedule
{
    internal class ScheduleHydraCommand
    {
        public Command GetBuildSearchTermsCommand()
        {
            var command = new Command("getsearchterms")
            {
                Description = "Get search terms."
            };

            var options = GetBuildSearchTermsCommandOptions();
            foreach (var option in options)
            {
                command.Add(option);
            }

            var commonOptions = GetCommonOptions();
            foreach (var option in commonOptions)
            {
                command.Add(option);
            }

            var searchOptions = GetSearchCommandOptions();
            foreach (var option in searchOptions)
            {
                command.Add(option);
            }

            command.SetAction(async parseResult =>
            {
                var logLevelOptionValue = parseResult.GetValue((Option<Logger.LogLevel>)commonOptions.First(o => o.Name == "loglevel"));
                var groupsFileOptionValue = parseResult.GetValue((Option<FileInfo>)options.First(o => o.Name == "groupsfile"));
                var showsFileOptionValue = parseResult.GetValue((Option<FileInfo>)options.First(o => o.Name == "showsfile"));
                var nzbHydra = CreateNzbHydra(parseResult, commonOptions, searchOptions);
                await BuildSearchTermsHandler(logLevelOptionValue, groupsFileOptionValue, showsFileOptionValue, nzbHydra);
            });

            return command;
        }

        public Command GetSearchCommand()
        {
            var command = new Command("search")
            {
                Description = "Start search."
            };

            var options = GetSearchCommandOptions();
            foreach (var option in options)
            {
                command.Add(option);
            }

            var commonOptions = GetCommonOptions();
            foreach (var option in commonOptions)
            {
                command.Add(option);
            }

            command.SetAction(async parseResult =>
            {
                var logLevelOptionValue = parseResult.GetValue((Option<Logger.LogLevel>)commonOptions.First(o => o.Name == "loglevel"));
                var nzbHydra = CreateNzbHydra(parseResult, commonOptions, options);
                await StartSearchHandler(logLevelOptionValue, nzbHydra);
            });

            return command;
        }

        private List<Option> GetSearchCommandOptions()
        {
            var options = new List<Option>();

            var outputOption = new Option<DirectoryInfo>("--output", "-o")
            {
                Description = "Path to save found nzb files to.",
                Required = true,
            };
            options.Add(outputOption);

            var addDefaultResolutionOption = new Option<bool>("--append-default-resolution", "-d")
            {
                Description = "If set to true, all search terms will be appended with the value provided in the default-resolution parameter",
            };
            addDefaultResolutionOption.DefaultValueFactory = _ => false;
            options.Add(addDefaultResolutionOption);

            var timestampFileOption = new Option<FileInfo?>("--timestampfile", "-t")
            {
                Description = "Path to the timestamp file.",
            };
            timestampFileOption.DefaultValueFactory = _ => null;
            options.Add(timestampFileOption);

            var searchFrequencyOption = new Option<int>("--search-frequency", "-f")
            {
                Description = "Minimum delay between searches in hours.",
            };
            searchFrequencyOption.DefaultValueFactory = _ => 72;
            options.Add(searchFrequencyOption);

            return options;
        }

        private List<Option> GetBuildSearchTermsCommandOptions()
        {
            var options = new List<Option>();

            var groupFilePathOption = new Option<FileInfo>("--groupsfile", "-e")
            {
                Description = "Path to the groups list file.",
                Required = true,
            };
            options.Add(groupFilePathOption);

            var showFilePathOption = new Option<FileInfo>("--showsfile", "-w")
            {
                Description = "Path to the shows list file.",
                Required = true,
            };
            options.Add(showFilePathOption);

            return options;
        }

        private List<Option> GetCommonOptions()
        {
            var options = new List<Option>();

            var logLevelOption = new Option<Logger.LogLevel>("--loglevel", "-l")
            {
                Required = false,
            };
            logLevelOption.DefaultValueFactory = _ => Logger.LogLevel.info;
            options.Add(logLevelOption);

            var searchTermsOption = new Option<FileInfo>("--searchterms", "-s")
            {
                Required = true,
                Description = "Path to the search terms file.",
            };
            options.Add(searchTermsOption);

            var nzbHydraUriOption = new Option<string>("--nzbhydra-uri", "-u")
            {
                Required = true,
                Description = "Endpoint under which nzbhydra is reachable.",
            };
            options.Add(nzbHydraUriOption);

            var apikeyOption = new Option<string>("--nzbhydra-api-key", "-k")
            {
                Required = true,
                Description = "Api key for nzbhydra.",
            };
            options.Add(apikeyOption);

            var minSizeOption = new Option<int>("--minsize", "-n")
            {
                Description = "Minimum found file size to include in search results.",
            };
            minSizeOption.DefaultValueFactory = _ => 1;
            options.Add(minSizeOption);

            var maxSizeOption = new Option<int>("--maxsize", "-m")
            {
                Description = "Maximum found file size to include in search results.",
            };
            maxSizeOption.DefaultValueFactory = _ => 900000;
            options.Add(maxSizeOption);

            var maxAgeOption = new Option<int>("--maxage", "-a")
            {
                Required = true,
                Description = "Maximum age of found items to include in search results.",
            };
            options.Add(maxAgeOption);

            var categoryOption = new Option<string>("--category", "-c")
            {
                Required = true,
                Description = "NzbHydra category to search.",
            };
            options.Add(categoryOption);

            var indexersOption = new Option<string>("--indexers", "-i")
            {
                Required = true,
                Description = "List of indexers to search.",
            };
            options.Add(indexersOption);

            var defaultResolutionOption = new Option<string>("--default-resolution", "-r")
            {
                Description = "Default resolution to search for. Only used if 'append-default-resolution' is set to true.",
            };
            defaultResolutionOption.DefaultValueFactory = _ => "1080p";
            options.Add(defaultResolutionOption);

            var requestCooldownOption = new Option<int>("--request-cooldown", "-q")
            {
                Description = "Delay between nzbhydra requests in seconds.",
            };
            requestCooldownOption.DefaultValueFactory = _ => 10;
            options.Add(requestCooldownOption);

            return options;
        }

        private async Task StartSearchHandler(Logger.LogLevel logLevelOptionValue, NzbHydra nzbHydraInstance)
        {
            Logger.MinLogLevel = logLevelOptionValue;


            Logger.WriteLog($"Checking last search run date.");
            if (nzbHydraInstance.LastSuccesfulSearch == DateTime.MinValue)
            {
                Logger.WriteLog($"No last successful search run date was set.", Logger.LogLevel.debug);
                nzbHydraInstance.LastSuccesfulSearch = await nzbHydraInstance.GetLastSearchTimestamp();
            }

            var lastSearchAge = DateTime.UtcNow.Subtract(nzbHydraInstance.LastSuccesfulSearch);
            var totalHoursSinceLastSearch = (int)(Math.Ceiling(lastSearchAge.TotalHours));
            Logger.WriteLog($"The last successful search run was performed {totalHoursSinceLastSearch} hours ago", Logger.LogLevel.debug);

            if (totalHoursSinceLastSearch < nzbHydraInstance.SearchFrequencyHours)
            {
                Logger.WriteLog($"The minimum search frequency threshold is {nzbHydraInstance.SearchFrequencyHours} hours. Aborting search.", Logger.LogLevel.debug);
                return;
            }

            await nzbHydraInstance.StartSearchAsync();
        }

        public async Task BuildSearchTermsHandler(Logger.LogLevel logLevelOptionValue, FileInfo groupsFileOptionValue, FileInfo showsFileOptionValue, NzbHydra nzbHydraInstance)
        {
            Logger.MinLogLevel = logLevelOptionValue;

            Logger.WriteLog($"Generating search terms.", Logger.LogLevel.debug);
            await nzbHydraInstance.GenerateSearchTerms(groupsFileOptionValue, showsFileOptionValue);
            Logger.WriteLog($"Finished search term generation.", Logger.LogLevel.debug);
        }

        private static NzbHydra CreateNzbHydra(ParseResult parseResult, List<Option> commonOptions, List<Option> searchOptions)
        {
            return new NzbHydra
            {
                NzbDirectory = parseResult.GetValue((Option<DirectoryInfo>)searchOptions.First(o => o.Name == "output")),
                SearchTermFilePath = parseResult.GetValue((Option<FileInfo>)commonOptions.First(o => o.Name == "searchterms")),
                TimestampFilePath = parseResult.GetValue((Option<FileInfo?>)searchOptions.First(o => o.Name == "timestampfile")),
                ApiKey = parseResult.GetValue((Option<string>)commonOptions.First(o => o.Name == "nzbhydra-api-key")),
                AppendResolution = parseResult.GetValue((Option<bool>)searchOptions.First(o => o.Name == "append-default-resolution")),
                DefaultResolution = parseResult.GetValue((Option<string>)commonOptions.First(o => o.Name == "default-resolution")),
                RequestCooldown = parseResult.GetValue((Option<int>)commonOptions.First(o => o.Name == "request-cooldown")),
                HydraUrl = parseResult.GetValue((Option<string>)commonOptions.First(o => o.Name == "nzbhydra-uri")),
                MinSize = parseResult.GetValue((Option<int>)commonOptions.First(o => o.Name == "minsize")),
                MaxSize = parseResult.GetValue((Option<int>)commonOptions.First(o => o.Name == "maxsize")),
                MaxAge = parseResult.GetValue((Option<int>)commonOptions.First(o => o.Name == "maxage")),
                SearchFrequencyHours = parseResult.GetValue((Option<int>)searchOptions.First(o => o.Name == "search-frequency")),
                Category = parseResult.GetValue((Option<string>)commonOptions.First(o => o.Name == "category")),
                Indexers = parseResult.GetValue((Option<string>)commonOptions.First(o => o.Name == "indexers"))
            };
        }
    }
}
