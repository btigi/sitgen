using PluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReplacePlugin
{
    public class ArchivesByPublishDateCommand : ICommand
    {
        public string Name { get => "archivesbypublishdate"; }
        public Phase Phase => Phase.PostProcess;
        public string Description { get => "Generate archive pages, based on the publish date of each file"; }

        private readonly List<(DateTime publishDate, string filename)> Data = new List<(DateTime publishDate, string filename)>();

        public void Init()
        {
            return;
        }

        public bool ShouldPublish(List<(string, string)> frontmatter, string content)
        {
            return true;
        }

        public string Execute(string filename, List<(string, string)> frontmatter, string content)
        {
            foreach (var item in frontmatter)
            {
                if (string.Equals(item.Item1, "publishdate", StringComparison.OrdinalIgnoreCase) && DateTime.TryParse(item.Item2, out DateTime date) && date < DateTime.UtcNow)
                {
                    if (DateTime.TryParse(item.Item2, out var dt))
                    {
                        Data.Add((dt, filename));
                    }
                    break;
                }
            }
            
            return content;
        }

        public void DoPostProcess(string destinationDirectory)
        {
            ////TODO: Generate multiple files, by date
            //
            //var templateText = File.ReadAllText(template);
            //var generatedText = string.Join("<br>", Data.Select(s => $"<a href=\"{s.filename}\"> {s.filename} at {s.publishDate}</a>"));
            //generatedText = templateText.Replace("{body}", generatedText);
            //
            //var indexFile = @$"{destinationDirectory}\archives.html";
            //File.WriteAllTextAsync(indexFile, generatedText);
        }
    }
}