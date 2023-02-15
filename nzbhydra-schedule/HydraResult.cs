using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace nzbhydra_schedule
{
    internal class HydraResult
    {
        public string Title { get; set; }
        public int Size { get; set; } = 0;
        public DateTime PubDate { get; set; } = DateTime.MinValue;
        public string Link { get; set; }
        public string Enclosure { get; set; }
        public string Category { get; set; }

        public HydraResult  (XElement element) {
            Title = string.IsNullOrWhiteSpace(element.Element("title")?.Value) ? string.Empty : element.Element("title")?.Value;

            Logger.WriteLog($"Result: {Title}", Logger.LogLevel.debug);

            int elSize;
            int.TryParse((element.Element("size")?.Value), out elSize);
            Size = elSize;

            DateTime pubDate;
            DateTime.TryParse((element.Element("pubDate")?.Value), out pubDate);
            PubDate = pubDate;

            Link = string.IsNullOrWhiteSpace(element.Element("link")?.Value) ? string.Empty : element.Element("link")?.Value;
            Enclosure = string.IsNullOrWhiteSpace(element.Element("enclosure")?.Value) ? string.Empty : element.Element("enclosure")?.Attribute("url").Value;
            Category = string.IsNullOrWhiteSpace(element.Element("category")?.Value) ? string.Empty : element.Element("category")?.Value;
        }

        public string GetNzbName()
        {
            return string.Join("_", Title.Split(Path.GetInvalidFileNameChars())) + ".nzb";
        }

    }
}
