using System.CommandLine;

namespace nzbhydra_schedule
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            Logger.WriteLog($"Starting NzbHydra Scheduler.");

            var rootCommand = new RootCommand("Schedules searches in NzbHydra.");

            var startCommand = new ScheduleHydraCommand();
            rootCommand.Add(startCommand.GetSearchCommand());
            rootCommand.Add(startCommand.GetBuildSearchTermsCommand());
            return await rootCommand.Parse(args).InvokeAsync();
        }
    }
}
