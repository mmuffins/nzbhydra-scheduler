using System;
using System.CommandLine;
using System.CommandLine.Binding;

namespace nzbhydra_schedule
{
    internal class ScheduleHydraCommand
    {
        public Command GetCommand()
        {
            var command = new Command("search")
            {
                Description = "Start search."
            };

            var logLevelOption = new Option<Logger.LogLevel>("--loglevel")
            {
                IsRequired = false,
            };
            logLevelOption.AddAlias("-l");
            logLevelOption.SetDefaultValue(Logger.LogLevel.info);
            command.AddOption(logLevelOption);

            var searchTermsOption = new Option<FileInfo>("--searchterms")
            {
                IsRequired = true,
                Description = "Path to the search terms file.",
            };
            searchTermsOption.AddAlias("-s");
            command.AddOption(searchTermsOption);

            var outputOption = new Option<DirectoryInfo>("--output")
            {
                IsRequired = true,
                Description = "Path to save found nzb files to."
            };
            outputOption.AddAlias("-o");
            command.AddOption(outputOption);

            var nzbHydraUriOption = new Option<string>("--nzbhydra-uri")
            {
                IsRequired = true,
                Description = "Endpoint under which nzbhydra is reachable."
            };
            nzbHydraUriOption.AddAlias("-u");
            command.AddOption(nzbHydraUriOption);

            var apikeyOption = new Option<string>("--nzbhydra-api-key")
            {
                IsRequired = true,
                Description = "Api key for nzbhydra."
            };
            apikeyOption.AddAlias("-k");
            command.AddOption(apikeyOption);
            
            var minSizeOption = new Option<int>("--minsize")
            {
                IsRequired = false,
                Description = "Minimum found file size to include in search results."
            };
            minSizeOption.AddAlias("-n");
            minSizeOption.SetDefaultValue(1);
            command.AddOption(minSizeOption);

            var maxSizeOption = new Option<int>("--maxsize")
            {
                IsRequired = false,
                Description = "Maximum found file size to include in search results."
            };
            maxSizeOption.AddAlias("-m");
            maxSizeOption.SetDefaultValue(900000);
            command.AddOption(maxSizeOption);

            var maxAgeOption = new Option<int>("--maxage")
            {
                IsRequired = true,
                Description = "Maximum age of found items to include in search results."
            };
            maxAgeOption.AddAlias("-a");
            command.AddOption(maxAgeOption);

            var categoryOption = new Option<string>("--category")
            {
                IsRequired = true,
                Description = "NzbHydra category to search."
            };
            categoryOption.AddAlias("-c");
            command.AddOption(categoryOption);

            var indexersOption = new Option<string>("--indexers")
            {
                IsRequired = true,
                Description = "List of indexers to search."
            };
            indexersOption.AddAlias("-i");
            command.AddOption(indexersOption);

            var defaultResolutionOption = new Option<string>("--default-resolution")
            {
                IsRequired = false,
                Description = "Default resolution to search for. Only used if 'append-default-resolution' is set to true."
            };
            defaultResolutionOption.AddAlias("-r");
            defaultResolutionOption.SetDefaultValue("1080p");
            command.AddOption(defaultResolutionOption);

            var addDefaultResolutionOption = new Option<bool>("--append-default-resolution")
            {
                IsRequired = false,
                Description = "If set to true, all search terms will be appended with the value provided in the default-resolution parameter"
            };
            addDefaultResolutionOption.AddAlias("-d");
            addDefaultResolutionOption.SetDefaultValue("1080p");
            command.AddOption(addDefaultResolutionOption);

            var requestCooldownOption = new Option<int>("--request-cooldown")
            {
                IsRequired = false,
                Description = "Delay between nzbhydra searches in seconds."
            };
            requestCooldownOption.AddAlias("-q");
            requestCooldownOption.SetDefaultValue(10);
            command.AddOption(requestCooldownOption);

            command.SetHandler(async (logLevelOptionValue, NzbHydra) => 
                {
                    await StartSearchHandler(logLevelOptionValue, NzbHydra);
                },
                logLevelOption, new NzbHydraBinder(searchTermsOption, outputOption, nzbHydraUriOption, apikeyOption, minSizeOption, maxSizeOption, maxAgeOption, defaultResolutionOption, addDefaultResolutionOption, requestCooldownOption, categoryOption, indexersOption)
            );

            return command;
        }

        private async Task StartSearchHandler(Logger.LogLevel logLevelOptionValue, NzbHydra nzbHydraInstance)
        {
            Logger.MinLogLevel = logLevelOptionValue;

            Logger.WriteLog($"Starting search.", Logger.LogLevel.debug);
            await nzbHydraInstance.GetNzbsAsync();
            //Logger.WriteLog(hydra.SearchTerms[0]);
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
