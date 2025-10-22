using System.CommandLine;
using Microsoft.Extensions.Logging;
using Tagly.Db;

namespace Tagly.Exporter;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var dbPathOption = new Option<FileInfo>(name: "--db")
        {
            Description = "The db file to read",
            Required = true
        };
        dbPathOption.Validators.Add(dir =>
        {
            if (!(dir.GetValueOrDefault<FileInfo>()?.Exists ?? false))
            {
                dir.AddError("Db file must exist");
            }
        });

        var outputPathOption = new Option<DirectoryInfo>(name: "--out")
        {
            Description = "The output directory for photos",
            Required = true
        };
        outputPathOption.Validators.Add(dir =>
        {
            if (!(dir.GetValueOrDefault<DirectoryInfo>()?.Exists ?? false))
            {
                dir.AddError("Directory path must exist");
            }
        });

        var removeFromDbOption = new Option<bool>(name: "--remove")
        {
            Description = "Remove processed items from db",
            Required = false,
            DefaultValueFactory = _ => false
        };
        
        var copyDbOption = new Option<bool>(name: "--copy-db")
        {
            Description = "Copies the database to the output directory",
            Required = false,
            DefaultValueFactory = _ => false
        };
        
        var jsonMetaOption = new Option<bool>(name: "--json-meta")
        {
            Description = "Creates a JSON metadata file in the output directory",
            Required = false,
            DefaultValueFactory = _ => false
        };

        var exportCommand = new Command("export");
        exportCommand.Options.Add(dbPathOption);
        exportCommand.Options.Add(outputPathOption);
        exportCommand.Options.Add(removeFromDbOption);
        exportCommand.Options.Add(copyDbOption);
        exportCommand.Options.Add(jsonMetaOption);

        var logger = LoggerFactory.Create(
            builder => builder.AddConsole());

        var log = logger.CreateLogger<Program>();
        
        exportCommand.SetAction(async (parseResult) =>
        {
            var copyDb = parseResult.GetValue(copyDbOption);
            var outputPath = parseResult.GetValue(outputPathOption)!;
            var dbPath = parseResult.GetValue(dbPathOption)!;
            var removeFromDb = parseResult.GetValue(removeFromDbOption)!;
            if (copyDb)
            {
                var dbOutputPath = Path.Combine(outputPath!.FullName, dbPath.Name);
                File.Copy(dbPath.FullName, dbOutputPath);
                log.LogInformation("Backed up db to {}", dbOutputPath);
            }
            
            var context = new TaglyContext(dbPath.FullName);
            var exporter = new Exporter(context, outputPath.FullName, logger.CreateLogger<Exporter>());
            await exporter.ExportAsync();
            if (removeFromDb)
            {
                await exporter.RemoveFromDbAsync();
            }
        });

        var listCommand = new Command("list");
        listCommand.Options.Add(dbPathOption);
        listCommand.SetAction(async parseResult =>
        {
            var dbPath = parseResult.GetValue(dbPathOption)!;
            var context = new TaglyContext(dbPath.FullName);
            await foreach (var photo in context.Photos)
            {
                Console.WriteLine($"{photo.Id} \t {photo.FileName} \t {photo.Date:dd/MM/yyyy HH:mm} \t {photo.Latitude}°N \t {photo.Longitude}°E \t {photo.Description} \t {photo.Created:dd/MM/yyyy HH:mm}");
            }
        });
        
        var rootCommand = new RootCommand("Tagly Exporter");
        rootCommand.Subcommands.Add(exportCommand);
        rootCommand.Subcommands.Add(listCommand);
        
        return await rootCommand.Parse(args).InvokeAsync();
    }
}