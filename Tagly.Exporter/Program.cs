﻿using System.CommandLine;
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

        var rootCommand = new RootCommand("Tagly Exporter");
        rootCommand.AddOption(dbPathOption);
        rootCommand.AddOption(outputPathOption);
        rootCommand.AddOption(removeFromDbOption);
        rootCommand.AddOption(copyDbOption);

        var logger = LoggerFactory.Create(
            builder => builder.AddConsole());

        rootCommand.SetHandler(async (dbPath, outputPath, removeFromDb, copyDb) =>
        {
            var context = new TaglyContext(dbPath.FullName);
            var exporter = new Exporter(context, outputPath.FullName, logger.CreateLogger<Exporter>());
            await exporter.ExportAsync();
            if (copyDb)
            {
                var dbOutputPath = Path.Combine(outputPath.FullName, dbPath.Name);
                File.Copy(dbPath.FullName, dbOutputPath);                
            }
            
            if (removeFromDb)
            {
                await exporter.RemoveFromDbAsync();
            }
        }, dbPathOption, outputPathOption, removeFromDbOption, copyDbOption);

        return await rootCommand.InvokeAsync(args);
    }
}