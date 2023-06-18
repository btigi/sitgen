using System.Collections.Generic;

namespace PluginBase
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        Phase Phase { get; }
        void Init();
        bool ShouldPublish(List<(string, string)> frontmatter, string content);
        string Execute(string filename, List<(string, string)> frontmatter, string content);
        void DoPostProcess(string destinationDirectory);
    }
}