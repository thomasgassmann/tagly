using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Mapsui;
using Mapsui.Projections;

namespace Tagly.App;

public static class GeoCode
{
    private class GeocodeResult
    {
        [JsonPropertyName("lat")] public required string Latitude { get; set; }

        [JsonPropertyName("lon")] public required string Longitude { get; set; }
    }

    public static async Task<MPoint?> GetPointAsync(string location)
    {
        var geocodeUrl = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(location)}&format=json";
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "tagly/1.0");
        var response = await httpClient.GetStringAsync(geocodeUrl);
        var results = JsonSerializer.Deserialize<List<GeocodeResult>>(response);
        if (results is not { Count: 1 })
        {
            return null;
        }

        var latitude = double.Parse(results[0].Latitude, CultureInfo.InvariantCulture);
        var longitude = double.Parse(results[0].Longitude, CultureInfo.InvariantCulture);
        var position = SphericalMercator.FromLonLat(new MPoint(longitude, latitude));
        return position;
    }
}