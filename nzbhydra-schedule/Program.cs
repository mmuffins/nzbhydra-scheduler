using static System.Net.Mime.MediaTypeNames;

namespace nzbhydra_schedule
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var time = TimeOnly.FromDateTime(DateTime.Now);
            Console.WriteLine($"It is now {time.ToString()}");

            var searchTermsFile = new FileInfo(@"C:\Users\email_000\source\nzbhydra-schedule\test\searchterms.txt");
            var nzbDirectory = new DirectoryInfo(@"C:\Users\email_000\source\nzbhydra-schedule\test\nzb");
            var minSize = 1;
            var maxSize = 900000;
            var defaultRes = "1080p";
            var appendResolution = false;
            var requestCooldown = 2;

            var hydra = new NzbHydra(nzbDirectory, searchTermsFile, "", "localhost:5076", appendResolution, defaultRes, requestCooldown, minSize, maxSize);
            await hydra.GetNzbsAsync();
        }
    }
}