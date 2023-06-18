using PluginBase;
using System;
using System.Collections.Generic;

namespace PublishDate
{
    public class PublishDateCommand : ICommand
    {
        public string Name { get => "publishdate"; }
        public Phase Phase => Phase.FileDeterminator;
        public string Description { get => "Prevent files with a future publish date being published"; }

        public void Init()
        {
            return;
        }

        public bool ShouldPublish(List<(string, string)> frontmatter, string content)
        {
            foreach (var item in frontmatter)
            {
                if (string.Equals(item.Item1, "publishdate", StringComparison.OrdinalIgnoreCase) && DateTime.TryParse(item.Item2, out DateTime date) && date > DateTime.UtcNow)
                {
                    return false;
                }
            }
            return true;
        }

        public string Execute(string filename, List<(string, string)> frontmatter, string content)
        {
            return content;
        }

        public void DoPostProcess(string destinationDirectory)
        {
            return;
        }
    }
}