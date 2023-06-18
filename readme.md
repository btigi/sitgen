[![Build status](https://ci.appveyor.com/api/projects/status/y8fnx41wd3ypnlx6?svg=true)](https://ci.appveyor.com/project/igi/sitgen)

---

# SitGen

A command-line driven plugin-enabled single-pass static site generator.


## Default Plugins

ArchivesByPublishDate
IndexGenerator
PublishDate
Replace
Tags


## Plugin Development

Implement the ICommand interface located in the PluginBase project.


## Usage
``` 
sitegen
```

The application accepts no parameters, configuration values are read from sitegen.json, located in the current directory.

## Download

You can [download](https://github.com/btigi/sitgen/releases/) the latest version of sitgen.


## Technologies

SitGen is written in C# Net Core 7.


## Compiling

To clone and run this application, you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/sitgen

# Go into the repository
$ cd sitgen

# Build  the app
$ dotnet build
```


## License

SitGen is licensed under [CC BY-NC 4.0](https://creativecommons.org/licenses/by-nc/4.0/)
