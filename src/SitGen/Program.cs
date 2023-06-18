using Markdig;
using PluginBase;
using Microsoft.Extensions.Configuration;
using SitGen.Model;
using SitGen;

static void GetFrontMatter(string rawText, out string markdownText, out List<(string, string)> frontMatter)
{
    markdownText = rawText;
    var frontMatterText = string.Empty;
    frontMatter = new List<(string, string)>();
    const string separator = "---";
    var frontMatterEnd = rawText.IndexOf(separator);
    if (frontMatterEnd != -1)
    {
        frontMatterText = rawText[..frontMatterEnd];
        markdownText = rawText[(frontMatterEnd + separator.Length)..];
    }

    var frontMatterItems = frontMatterText.Split(Environment.NewLine);
    foreach (var item in frontMatterItems)
    {
        var frontMatterParts = item.Split(":", 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (frontMatterParts.Length > 1)
        {
            frontMatter.Add((frontMatterParts[0], frontMatterParts[1]));
        }
    }
}

try
{
    var appSettings = new AppSettings();
    var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    var configuration = builder.Build();
    ConfigurationBinder.Bind(configuration.GetSection("AppSettings"), appSettings);

    var pluginPaths = configuration
        .GetSection("Plugins")
        .GetChildren()
        .Select(x => x.Value)
        .ToArray();

    var commands = pluginPaths.SelectMany(pluginPath =>
    {
        var pluginAssembly = PluginLoader.LoadPlugin(pluginPath);
        return PluginLoader.CreateCommands(pluginAssembly);
    }).ToList();

    var sourceDirectory = appSettings.SourceDirectory;
    var destinationDirectory = appSettings.DestinationDirectory;

    Directory.CreateDirectory(destinationDirectory);

    foreach (ICommand command in commands)
    {
        command.Init();
    }

    foreach (var file in Directory.GetFiles(sourceDirectory, "*.md"))
    {
        var filename = Path.GetFileName(file);
        Console.WriteLine();
        Console.WriteLine($"Processing {filename}");

        var rawText = await File.ReadAllTextAsync(file);

        GetFrontMatter(rawText, out string markdownText, out List<(string, string)> frontMatter);

        Console.WriteLine("Running file determinator commands");
        var shouldPublish = true;
        foreach (ICommand command in commands.Where(w => w.Phase == Phase.FileDeterminator))
        {
            Console.WriteLine($"{command.Name}");
            if (!command.ShouldPublish(frontMatter, markdownText))
            {
                shouldPublish = false;
                break;
            }
        }

        if (!shouldPublish)
        {
            continue;
        }


        Console.WriteLine("Running pre-build commands");
        foreach (var command in commands.Where(w => w.Phase == Phase.PreBuild))
        {
            Console.WriteLine($"{command.Name}");
            markdownText = command.Execute(filename, frontMatter, markdownText);
        }

        Console.WriteLine("Building");
        var htmlText = Markdown.ToHtml(markdownText);

        Console.WriteLine("Running post-build commands");
        foreach (var command in commands.Where(w => w.Phase == Phase.PostBuild))
        {
            Console.WriteLine($"{command.Name}");
            htmlText = command.Execute(filename, frontMatter, htmlText);
        }


        var singlePageTemplateText = File.ReadAllText(appSettings.SinglePageTemplate);
        var mergedText = singlePageTemplateText.Replace("{body}", htmlText);

        Console.WriteLine("Running post-merge commands");
        foreach (var command in commands.Where(w => w.Phase == Phase.PostMerge))
        {
            Console.WriteLine($"{command.Name}");
            mergedText = command.Execute(filename, frontMatter, mergedText);
        }

        Console.WriteLine("Running post-process commands (stage 1)");
        foreach (var command in commands.Where(w => w.Phase == Phase.PostProcess))
        {
            mergedText = command.Execute(filename, frontMatter, mergedText);
        }

        var basefilename = Path.GetFileNameWithoutExtension(file);
        var destinationFile = @$"{destinationDirectory}\{basefilename}.html".ToLower();
        await File.WriteAllTextAsync(destinationFile, mergedText);
    }

    Console.WriteLine("Running post-process commands (stage 2)");
    foreach (var command in commands.Where(w => w.Phase == Phase.PostProcess))
    {
        command.DoPostProcess(destinationDirectory);
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}