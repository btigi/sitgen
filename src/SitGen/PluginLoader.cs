using SitGen;
using PluginBase;
using System.Reflection;

namespace SitGen
{
    static class PluginLoader
    {
        public static Assembly LoadPlugin(string path)
        {
            Console.WriteLine();
            var pluginLocation = path;
            if (!Path.IsPathRooted(path))
            {
                var root = AppContext.BaseDirectory;
                pluginLocation = Path.GetFullPath(Path.Combine(root, path.Replace('\\', Path.DirectorySeparatorChar)));
            }

            Console.WriteLine($"Loading commands from: {pluginLocation}");
            var loadContext = new PluginLoadContext(pluginLocation);
            return loadContext.LoadFromAssemblyName(AssemblyName.GetAssemblyName(pluginLocation));
        }

        public static IEnumerable<ICommand> CreateCommands(Assembly assembly)
        {
            var count = 0;
            foreach (var type in assembly.GetTypes().Where(type => typeof(ICommand).IsAssignableFrom(type)))
            {
                if (Activator.CreateInstance(type) is ICommand result)
                {
                    count++;
                    yield return result;
                }
            }

            if (count == 0)
            {
                var availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                throw new ApplicationException(
                    $"Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }
    }
}