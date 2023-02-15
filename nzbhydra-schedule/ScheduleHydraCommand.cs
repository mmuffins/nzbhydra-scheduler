using System;
using System.CommandLine;
using System.CommandLine.Binding;

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


            command.SetHandler(async (logLevelOptionValue, groupsFileOptionValue, showsFileOptionValue, NzbHydra) =>
            {
                await BuildSearchTermsHandler(logLevelOptionValue, groupsFileOptionValue, showsFileOptionValue, NzbHydra);
            },
                (Option<Logger.LogLevel>)commonOptions.First(o => o.Name == "loglevel"),
                (Option<FileInfo>)options.First(o => o.Name == "groupsfile"),
                (Option<FileInfo>)options.First(o => o.Name == "showsfile"),
                new NzbHydraBinder(
                    (Option<FileInfo>)commonOptions.First(o => o.Name == "searchterms"),
                    (Option<DirectoryInfo>)searchOptions.First(o => o.Name == "output"),
                    (Option<string>)commonOptions.First(o => o.Name == "nzbhydra-uri"),
                    (Option<string>)commonOptions.First(o => o.Name == "nzbhydra-api-key"),
                    (Option<int>)commonOptions.First(o => o.Name == "minsize"),
                    (Option<int>)commonOptions.First(o => o.Name == "maxsize"),
                    (Option<int>)commonOptions.First(o => o.Name == "maxage"),
                    (Option<string>)commonOptions.First(o => o.Name == "default-resolution"),
                    (Option<bool>)searchOptions.First(o => o.Name == "append-default-resolution"),
                    (Option<int>)commonOptions.First(o => o.Name == "request-cooldown"),
                    (Option<string>)commonOptions.First(o => o.Name == "category"),
                    (Option<string>)commonOptions.First(o => o.Name == "indexers")
                )
            );

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

            command.SetHandler(async (logLevelOptionValue, NzbHydra) =>
                {
                    await StartSearchHandler(logLevelOptionValue, NzbHydra);
                },
                (Option<Logger.LogLevel>)commonOptions.First(o => o.Name == "loglevel"),
                new NzbHydraBinder(
                    (Option<FileInfo>)commonOptions.First(o => o.Name == "searchterms"),
                    (Option<DirectoryInfo>)options.First(o => o.Name == "output"),
                    (Option<string>)commonOptions.First(o => o.Name == "nzbhydra-uri"),
                    (Option<string>)commonOptions.First(o => o.Name == "nzbhydra-api-key"),
                    (Option<int>)commonOptions.First(o => o.Name == "minsize"),
                    (Option<int>)commonOptions.First(o => o.Name == "maxsize"),
                    (Option<int>)commonOptions.First(o => o.Name == "maxage"),
                    (Option<string>)commonOptions.First(o => o.Name == "default-resolution"),
                    (Option<bool>)options.First(o => o.Name == "append-default-resolution"),
                    (Option<int>)commonOptions.First(o => o.Name == "request-cooldown"),
                    (Option<string>)commonOptions.First(o => o.Name == "category"),
                    (Option<string>)commonOptions.First(o => o.Name == "indexers")
                )
            );

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
                Description = "Delay between nzbhydra searches in seconds."
            };
            requestCooldownOption.AddAlias("-q");
            requestCooldownOption.SetDefaultValue(10);
            options.Add(requestCooldownOption);

            return options;
        }

        private async Task StartSearchHandler(Logger.LogLevel logLevelOptionValue, NzbHydra nzbHydraInstance)
        {
            Logger.MinLogLevel = logLevelOptionValue;

            Logger.WriteLog($"Starting search.", Logger.LogLevel.debug);
            await nzbHydraInstance.StartSearchAsync();
            Logger.WriteLog($"Finished search.", Logger.LogLevel.debug);
        }

        public async Task BuildSearchTermsHandler(Logger.LogLevel logLevelOptionValue, FileInfo groupsFileOptionValue, FileInfo showsFileOptionValue, NzbHydra nzbHydraInstance)
        {
            Logger.MinLogLevel = logLevelOptionValue;

            Logger.WriteLog($"Generating search terms.", Logger.LogLevel.debug);
            await nzbHydraInstance.GenerateSearchTerms(groupsFileOptionValue, showsFileOptionValue);
            Logger.WriteLog($"Finished search term generation.", Logger.LogLevel.debug);
        }

        public class NzbHydraBinder : BinderBase<NzbHydra>
        {
            private readonly Option<FileInfo> _searchTermsOption;
            private readonly Option<DirectoryInfo> _outputOption;
            private readonly Option<string> _nzbHydraUriOption;
            private readonly Option<string> _apikeyOption;
            private readonly Option<int> _minSizeOption;
            private readonly Option<int> _maxSizeOption;
            private readonly Option<int> _maxAgeOption;
            private readonly Option<string> _defaultResolutionOption;
            private readonly Option<bool> _addDefaultResolutionOption;
            private readonly Option<int> _requestCooldownOption;
            private readonly Option<string> _categoryOption;
            private readonly Option<string> _indexersOption;

            public NzbHydraBinder(Option<FileInfo> searchTermsOption, Option<DirectoryInfo> outputOption, Option<string> nzbHydraUriOption, Option<string> apikeyOption, Option<int> minSizeOption, Option<int> maxSizeOption, Option<int> maxAgeOption, Option<string> defaultResolutionOption, Option<bool> addDefaultResolutionOption, Option<int> requestCooldownOption, Option<string> categoryOption, Option<string> indexersOption)
            {
                _searchTermsOption = searchTermsOption;
                _outputOption = outputOption;
                _nzbHydraUriOption= nzbHydraUriOption;
                _apikeyOption = apikeyOption;
                _minSizeOption = minSizeOption;
                _maxSizeOption = maxSizeOption;
                _maxAgeOption = maxAgeOption;
                _defaultResolutionOption = defaultResolutionOption;
                _addDefaultResolutionOption = addDefaultResolutionOption;
                _requestCooldownOption = requestCooldownOption;
                _categoryOption = categoryOption;
                _indexersOption = indexersOption;
            }

            protected override NzbHydra GetBoundValue(BindingContext bindingContext) =>
                new NzbHydra
                {
                    NzbDirectory = bindingContext.ParseResult.GetValueForOption(_outputOption),
                    SearchTermFilePath = bindingContext.ParseResult.GetValueForOption(_searchTermsOption),
                    ApiKey = bindingContext.ParseResult.GetValueForOption(_apikeyOption),
                    AppendResolution = bindingContext.ParseResult.GetValueForOption(_addDefaultResolutionOption),
                    DefaultResolution = bindingContext.ParseResult.GetValueForOption(_defaultResolutionOption),
                    RequestCooldown = bindingContext.ParseResult.GetValueForOption(_requestCooldownOption),
                    HydraUrl = bindingContext.ParseResult.GetValueForOption(_nzbHydraUriOption),
                    MinSize = bindingContext.ParseResult.GetValueForOption(_minSizeOption),
                    MaxSize = bindingContext.ParseResult.GetValueForOption(_maxSizeOption),
                    MaxAge = bindingContext.ParseResult.GetValueForOption(_maxAgeOption),
                    Category = bindingContext.ParseResult.GetValueForOption(_categoryOption),
                    Indexers = bindingContext.ParseResult.GetValueForOption(_indexersOption)
                };
        }
    }
}
