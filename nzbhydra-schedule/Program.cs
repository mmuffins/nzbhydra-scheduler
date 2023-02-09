using System.CommandLine;
using static System.Net.Mime.MediaTypeNames;

namespace nzbhydra_schedule
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            Logger.WriteLog($"Starting NzbHydra Scheduler.");

            var rootCommand = new RootCommand()
            {
                Name = "NzbHydraScheduler",
                Description = "Schedules searches in NzbHydra."
            };

            var startCommand = new ScheduleHydraCommand();
            rootCommand.Add(startCommand.GetSearchCommand());
            rootCommand.Add(startCommand.GetBuildSearchTermsCommand());
            return await rootCommand.InvokeAsync(args);
        }
    }
}