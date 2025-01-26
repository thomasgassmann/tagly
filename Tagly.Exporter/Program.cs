using System.CommandLine;
using Microsoft.Extensions.Logging;
using Tagly.Db;

namespace Tagly.Exporter;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var dbPathOption = new Option<FileInfo>(
            name: "--db",
            description: "The db file to read")
        {
            IsRequired = true
        };
        dbPathOption.AddValidator(dir =>
        {
            if (!(dir.GetValueOrDefault<FileInfo>()?.Exists ?? false))
            {
                dir.ErrorMessage = "Db file must exist";
            }
        });

        var outputPathOption = new Option<DirectoryInfo>(
            name: "--out",
            description: "The output directory for photos")
        {
            IsRequired = true
        };
        outputPathOption.AddValidator(dir =>
        {
            if (!(dir.GetValueOrDefault<DirectoryInfo>()?.Exists ?? false))
            {
                dir.ErrorMessage = "Directory path must exist";
            }
        });

        var removeFromDbOption = new Option<bool>(
            name: "--remove",
            description: "Remove processed items from db")
        {
            IsRequired = false
        };
        removeFromDbOption.SetDefaultValue(false);
        
        var copyDbOption = new Option<bool>(
            name: "--copy-db",
            description: "Copies the database to the output directory")
        {
            IsRequired = false
        };
        copyDbOption.SetDefaultValue(false);
        
        var jsonMetaOption = new Option<bool>(
            name: "--json-meta",
            description: "Creates a JSON metadata file in the output directory")
        {
            IsRequired = false
        };
        jsonMetaOption.SetDefaultValue(false);

        var exportCommand = new Command("export");
        exportCommand.AddOption(dbPathOption);
        exportCommand.AddOption(outputPathOption);
        exportCommand.AddOption(removeFromDbOption);
        exportCommand.AddOption(copyDbOption);
        exportCommand.AddOption(jsonMetaOption);

        var logger = LoggerFactory.Create(
            builder => builder.AddConsole());

        var log = logger.CreateLogger<Program>();
        
        exportCommand.SetHandler(async (dbPath, outputPath, removeFromDb, copyDb) =>
        {
            if (copyDb)
            {
                var dbOutputPath = Path.Combine(outputPath.FullName, dbPath.Name);
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
        }, dbPathOption, outputPathOption, removeFromDbOption, copyDbOption);

        var listCommand = new Command("list");
        listCommand.AddOption(dbPathOption);
        listCommand.SetHandler(async (dbPath) =>
        {
            var context = new TaglyContext(dbPath.FullName);
            await foreach (var photo in context.Photos)
            {
                Console.WriteLine($"{photo.Id} \t {photo.FileName} \t {photo.Date:dd/MM/yyyy HH:mm} \t {photo.Latitude}°N \t {photo.Longitude}°E \t {photo.Description} \t {photo.Created:dd/MM/yyyy HH:mm}");
            }
        }, dbPathOption);
        
        var rootCommand = new RootCommand("Tagly Exporter");
        rootCommand.AddCommand(exportCommand);
        rootCommand.AddCommand(listCommand);
        
        return await rootCommand.InvokeAsync(args);
    }
}