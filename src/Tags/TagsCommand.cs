using Microsoft.Extensions.Configuration;
using PluginBase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Tags.Model;

namespace Tags
{
    public class TagsCommand : ICommand
    {
        public string Name { get => "tags"; }
        public Phase Phase => Phase.PostProcess;
        public string Description { get => "Provide tag support"; }

        private AppSettings appSettings;
        private readonly List<(string filename, List<string> tags)> Data = new List<(string filename, List<string> tags)>();

        public void Init()
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            appSettings = new AppSettings();
            var builder = new ConfigurationBuilder()
                            .SetBasePath(location)
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuration = builder.Build();
            ConfigurationBinder.Bind(configuration.GetSection("AppSettings"), appSettings);
        }

        public bool ShouldPublish(List<(string, string)> frontmatter, string content)
        {
            return true;
        }

        private string[] ExtractTags(List<(string, string)> frontmatter)
        {
            foreach (var item in frontmatter)
            {
                if (string.Equals(item.Item1, "tags", StringComparison.OrdinalIgnoreCase))
                {
                    return item.Item2.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                }
            }
            return Array.Empty<string>();
        }

        public string Execute(string filename, List<(string, string)> frontmatter, string content)
        {
            var tags = ExtractTags(frontmatter);
            Data.Add((filename, tags.ToList()));
            var sb = new StringBuilder();
            for (int i = 0; i < tags.Length; i++)
            {
                var sepchar = i == tags.Length - 1 ? "" : " | ";
                sb.Append($"<a href=\"tag-{tags[i].ToLower()}.html\">{tags[i].ToLower()}</a>{sepchar}");
            }
            content = content.Replace("{tags}", sb.ToString());

            return content;
        }

        public void DoPostProcess(string destinationDirectory)
        {
            var allTags = new List<string>();
            foreach (var entry in Data)
            {
                allTags.AddRange(entry.tags);
            }

            foreach (var entry in allTags)
            {
                var filesWithThisTags = Data.Where(w => w.tags.Contains(entry)).ToList();
                var sb = new StringBuilder();
                sb.Append($"Showing entries tagged '{entry}'<br>");
                foreach (var file in filesWithThisTags)
                {
                    sb.Append($"<a href=\"{Path.ChangeExtension(file.filename.ToLower(), ".html")}\"> {CultureInfo.InvariantCulture.TextInfo.ToTitleCase(Path.GetFileNameWithoutExtension(file.filename).ToLower())}</a>");
                }

                var template = appSettings.TagPageTemplate;
                var templateText = File.ReadAllText(template);
                var generatedText = templateText.Replace("{body}", sb.ToString());
                var tagFile = @$"{destinationDirectory}\tag-{entry}.html".ToLower();
                File.WriteAllTextAsync(tagFile, generatedText);
            }
        }
    }
}