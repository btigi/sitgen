using Microsoft.Extensions.Configuration;
using PluginBase;
using IndexGenerator.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System;
using System.Globalization;

namespace IndexGeneratorPlugin
{
    public class IndexGeneratorCommand : ICommand
    {
        public string Name { get => "indexgenerator"; }
        public Phase Phase => Phase.PostProcess;
        public string Description { get => "Generate an index page"; }

        private AppSettings appSettings;
        private readonly List<(int index, string outputfile, string title)> Data = new List<(int index, string outputfile, string title)>();

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

        private int FindIndexOrder(List<(string, string)> frontmatter)
        {
            foreach (var item in frontmatter)
            {
                if (string.Equals(item.Item1, "index", StringComparison.OrdinalIgnoreCase))
                {
                    return Convert.ToInt32(item.Item2);
                }
            }
            return 0;
        }

        public string Execute(string filename, List<(string, string)> frontmatter, string content)
        {
            var index = FindIndexOrder(frontmatter);

            Data.Add((index, Path.ChangeExtension(filename, ".html"), Path.GetFileNameWithoutExtension(filename)));

            return content;
        }

        public void DoPostProcess(string destinationDirectory)
        {
            var template = appSettings.IndexPageTemplate;
            var templateText = File.ReadAllText(template);
            var generatedText = string.Join("<br>", Data.OrderBy(o => o.index).Select(s => $"<a href=\"{s.outputfile}\"> {CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.title.ToLower())}</a>"));
            generatedText = templateText.Replace("{body}", generatedText);

            var indexFile = @$"{destinationDirectory}\index.html".ToLower();
            File.WriteAllTextAsync(indexFile, generatedText);
        }
    }
}