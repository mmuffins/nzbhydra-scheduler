using System;
using System.CommandLine;
using System.CommandLine.Invocation;
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
                command.AddOption(option);
            }

            var commonOptions = GetCommonOptions();
            foreach (var option in commonOptions)
            {
                command.AddOption(option);
            }

            var searchOptions = GetSearchCommandOptions();
            foreach (var option in searchOptions)
            {
                command.AddOption(option);
            }


            command.SetHandler(async context =>
            {
                var parseResult = context.ParseResult;
                var logLevelOptionValue = parseResult.GetValueForOption((Option<Logger.LogLevel>)commonOptions.First(o => o.Name == "loglevel"));
                var groupsFileOptionValue = parseResult.GetValueForOption((Option<FileInfo>)options.First(o => o.Name == "groupsfile"));
                var showsFileOptionValue = parseResult.GetValueForOption((Option<FileInfo>)options.First(o => o.Name == "showsfile"));
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
                command.AddOption(option);
            }

            var commonOptions = GetCommonOptions();
            foreach (var option in commonOptions)
            {
                command.AddOption(option);
            }

            command.SetHandler(async context =>
            {
                var parseResult = context.ParseResult;
                var logLevelOptionValue = parseResult.GetValueForOption((Option<Logger.LogLevel>)commonOptions.First(o => o.Name == "loglevel"));
                var nzbHydra = CreateNzbHydra(parseResult, commonOptions, options);
                await StartSearchHandler(logLevelOptionValue, nzbHydra);
            });

            return command;
        }

        private List<Option> GetSearchCommandOptions()
        {
            var options = new List<Option>();

            var outputOption = new Option<DirectoryInfo>("--output")
            {
                IsRequired = true,
                Description = "Path to save found nzb files to."
            };
            outputOption.AddAlias("-o");
            options.Add(outputOption);

            var addDefaultResolutionOption = new Option<bool>("--append-default-resolution")
            {
                IsRequired = false,
                Description = "If set to true, all search terms will be appended with the value provided in the default-resolution parameter"
            };
            addDefaultResolutionOption.AddAlias("-d");
            addDefaultResolutionOption.SetDefaultValue("1080p");
            options.Add(addDefaultResolutionOption);
            
            var timestampFileOption = new Option<FileInfo>("--timestampfile")
            {
                IsRequired = false,
                Description = "Path to the timestamp file.",
            };
            timestampFileOption.AddAlias("-t");
            timestampFileOption.SetDefaultValue(null);
            options.Add(timestampFileOption);

            var searchFrequencyOption = new Option<int>("--search-frequency")
            {
                IsRequired = false,
                Description = "Minimum delay between searches in hours."
            };
            searchFrequencyOption.AddAlias("-f");
            searchFrequencyOption.SetDefaultValue(72);
            options.Add(searchFrequencyOption);

            return options;
        }

        private List<Option> GetBuildSearchTermsCommandOptions()
        {
            var options = new List<Option>();

            var groupFilePathOption = new Option<FileInfo>("--groupsfile")
            {
                IsRequired = true,
                Description = "Path to the groups list file.",
            };
            groupFilePathOption.AddAlias("-e");
            options.Add(groupFilePathOption);

            var showFilePathOption = new Option<FileInfo>("--showsfile")
            {
                IsRequired = true,
                Description = "Path to the shows list file.",
            };
            showFilePathOption.AddAlias("-w");
            options.Add(showFilePathOption);

            return options;
        }

        private List<Option> GetCommonOptions()
        {
            var options = new List<Option>();

            var logLevelOption = new Option<Logger.LogLevel>("--loglevel")
            {
                IsRequired = false,
            };
            logLevelOption.AddAlias("-l");
            logLevelOption.SetDefaultValue(Logger.LogLevel.info);
            options.Add(logLevelOption);

            var searchTermsOption = new Option<FileInfo>("--searchterms")
            {
                IsRequired = true,
                Description = "Path to the search terms file.",
            };
            searchTermsOption.AddAlias("-s");
            options.Add(searchTermsOption);


            var nzbHydraUriOption = new Option<string>("--nzbhydra-uri")
            {
                IsRequired = true,
                Description = "Endpoint under which nzbhydra is reachable."
            };
            nzbHydraUriOption.AddAlias("-u");
            options.Add(nzbHydraUriOption);

            var apikeyOption = new Option<string>("--nzbhydra-api-key")
            {
                IsRequired = true,
                Description = "Api key for nzbhydra."
            };
            apikeyOption.AddAlias("-k");
            options.Add(apikeyOption);

            var minSizeOption = new Option<int>("--minsize")
            {
                IsRequired = false,
                Description = "Minimum found file size to include in search results."
            };
            minSizeOption.AddAlias("-n");
            minSizeOption.SetDefaultValue(1);
            options.Add(minSizeOption);

            var maxSizeOption = new Option<int>("--maxsize")
            {
                IsRequired = false,
                Description = "Maximum found file size to include in search results."
            };
            maxSizeOption.AddAlias("-m");
            maxSizeOption.SetDefaultValue(900000);
            options.Add(maxSizeOption);

            var maxAgeOption = new Option<int>("--maxage")
            {
                IsRequired = true,
                Description = "Maximum age of found items to include in search results."
            };
            maxAgeOption.AddAlias("-a");
            options.Add(maxAgeOption);

            var categoryOption = new Option<string>("--category")
            {
                IsRequired = true,
                Description = "NzbHydra category to search."
            };
            categoryOption.AddAlias("-c");
            options.Add(categoryOption);

            var indexersOption = new Option<string>("--indexers")
            {
                IsRequired = true,
                Description = "List of indexers to search."
            };
            indexersOption.AddAlias("-i");
            options.Add(indexersOption);

            var defaultResolutionOption = new Option<string>("--default-resolution")
            {
                IsRequired = false,
                Description = "Default resolution to search for. Only used if 'append-default-resolution' is set to true."
            };
            defaultResolutionOption.AddAlias("-r");
            defaultResolutionOption.SetDefaultValue("1080p");
            options.Add(defaultResolutionOption);

            var requestCooldownOption = new Option<int>("--request-cooldown")
            {
                IsRequired = false,
                Description = "Delay between nzbhydra requests in seconds."
            };
            requestCooldownOption.AddAlias("-q");
            requestCooldownOption.SetDefaultValue(10);
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
                NzbDirectory = parseResult.GetValueForOption((Option<DirectoryInfo>)searchOptions.First(o => o.Name == "output")),
                SearchTermFilePath = parseResult.GetValueForOption((Option<FileInfo>)commonOptions.First(o => o.Name == "searchterms")),
                TimestampFilePath = parseResult.GetValueForOption((Option<FileInfo>)searchOptions.First(o => o.Name == "timestampfile")),
                ApiKey = parseResult.GetValueForOption((Option<string>)commonOptions.First(o => o.Name == "nzbhydra-api-key")),
                AppendResolution = parseResult.GetValueForOption((Option<bool>)searchOptions.First(o => o.Name == "append-default-resolution")),
                DefaultResolution = parseResult.GetValueForOption((Option<string>)commonOptions.First(o => o.Name == "default-resolution")),
                RequestCooldown = parseResult.GetValueForOption((Option<int>)commonOptions.First(o => o.Name == "request-cooldown")),
                HydraUrl = parseResult.GetValueForOption((Option<string>)commonOptions.First(o => o.Name == "nzbhydra-uri")),
                MinSize = parseResult.GetValueForOption((Option<int>)commonOptions.First(o => o.Name == "minsize")),
                MaxSize = parseResult.GetValueForOption((Option<int>)commonOptions.First(o => o.Name == "maxsize")),
                MaxAge = parseResult.GetValueForOption((Option<int>)commonOptions.First(o => o.Name == "maxage")),
                SearchFrequencyHours = parseResult.GetValueForOption((Option<int>)searchOptions.First(o => o.Name == "search-frequency")),
                Category = parseResult.GetValueForOption((Option<string>)commonOptions.First(o => o.Name == "category")),
                Indexers = parseResult.GetValueForOption((Option<string>)commonOptions.First(o => o.Name == "indexers"))
            };
        }
    }
}
