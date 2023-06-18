using PluginBase;
using System;
using System.Collections.Generic;

namespace Replace
{
    public class ReplaceCommand : ICommand
    {
        public string Name { get => "replace"; }
        public Phase Phase => Phase.PostMerge;
        public string Description { get => "General text replacement plugin"; }

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
                if (string.Equals(item.Item1, "replace", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = item.Item2.Split(":");
                    content = content.Replace(parts[0], parts[1]);
                }
            }
            return content;
        }

        public void DoPostProcess(string destinationDirectory)
        {
            return;
        }
    }
}