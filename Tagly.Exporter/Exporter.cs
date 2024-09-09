using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata.Profiles.Iptc;
using SixLabors.ImageSharp.Metadata.Profiles.Xmp;
using Tagly.Db;

namespace Tagly.Exporter;

public class Exporter(TaglyContext context, string outputPath, ILogger<Exporter> logger)
{
    public async Task Export(bool removeFromDb)
    {
        var removedItems = new List<StoredPhoto>();
        await foreach (var item in context.Photos)
        {
            var fileName = Path.Combine(outputPath, $"{item.Id}-{item.FileName}");
            logger.LogInformation($"Output item with ID {item.Id} to {fileName}");
            if (File.Exists(fileName))
            {
                logger.LogError($"File already exists: {fileName}");
                throw new ArgumentException($"File {fileName} already exists.");
            }

            await ExportSingle(item, fileName);
            removedItems.Add(item);
        }

        if (removeFromDb)
        {
            context.Photos.RemoveRange(removedItems);
            await context.SaveChangesAsync();
        }
    }

    private async Task ExportSingle(StoredPhoto storedPhoto, string fileName)
    {
        using var stream = new MemoryStream(storedPhoto.Data);
        using var image = await Image.LoadAsync(stream);

        // now add Description, Longitude, Latitude and Date to photo
        if (image.Metadata.ExifProfile == null)
        {
            logger.LogWarning($"EXIF Metadata for file {fileName} does not exist. Creating it.");
            image.Metadata.ExifProfile = new ExifProfile();
        }

        if (storedPhoto.Date.HasValue)
        {
            const string format = "yyyy:MM:dd HH:mm:ss";
            image.Metadata.ExifProfile.SetValue(ExifTag.DateTimeOriginal, storedPhoto.Date.Value.ToString(format));
        }

        if (storedPhoto is { Longitude: not null, Latitude: not null } &&
            !(storedPhoto.Longitude.Value == 0 && storedPhoto.Latitude.Value == 0))
        {
            image.Metadata.ExifProfile.SetValue(ExifTag.GPSLongitude,
                ConvertDecimalToRational(Math.Abs(storedPhoto.Longitude.Value)));
            image.Metadata.ExifProfile.SetValue(ExifTag.GPSLatitude,
                ConvertDecimalToRational(Math.Abs(storedPhoto.Latitude.Value)));

            image.Metadata.ExifProfile.SetValue(ExifTag.GPSLongitudeRef,
                Math.Sign(storedPhoto.Longitude.Value) > 0 ? "E" : "W");
            image.Metadata.ExifProfile.SetValue(ExifTag.GPSLatitudeRef,
                Math.Sign(storedPhoto.Latitude.Value) > 0 ? "N" : "S");
        }

        if (!string.IsNullOrWhiteSpace(storedPhoto.Description))
        {
            image.Metadata.ExifProfile.SetValue(ExifTag.ImageDescription, storedPhoto.Description);
            image.Metadata.ExifProfile.SetValue(ExifTag.XPTitle, storedPhoto.Description);
            if (image.Metadata.IptcProfile == null)
            {
                image.Metadata.IptcProfile = new IptcProfile();
            }

            image.Metadata.IptcProfile.SetValue(IptcTag.Caption, storedPhoto.Description);
        }

        await image.SaveAsync(fileName);
    }

    private static Rational[] ConvertDecimalToRational(double decimalValue)
    {
        var degrees = (int)Math.Floor(decimalValue);
        var remainingMinutes = (decimalValue - degrees) * 60;
        var minutes = (int)Math.Floor(remainingMinutes);
        var seconds = (remainingMinutes - minutes) * 60;
        var value = new Rational[] { new Rational(degrees), new Rational(minutes), new Rational(seconds) };
        return value;
    }
}