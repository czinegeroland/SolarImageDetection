using CsvHelper;
using MLModel1_WebApi1.Models;
using System.Globalization;
using System.IO.Compression;

namespace MLModel1_WebApi1.Services
{
    public static class SolarImageDownloadHelper
    {
        public static async Task DownloadSatelliteImages()
        {
            var coordinates = ReadCoordinatesFromCsv("solar_coordinates.csv");

            string apiKey = "**********************************";
            string outputZipPath = "SatelliteImages.zip";

            await DownloadSatelliteImages(coordinates, apiKey, outputZipPath);
        }

        private static async Task DownloadSatelliteImages(List<Coordinate> coordinates, string apiKey, string outputZipPath)
        {
            using var httpClient = new HttpClient();
            using var zipArchive = new ZipArchive(File.Open(outputZipPath, FileMode.Create), ZipArchiveMode.Create);

            var counter = 1;
            var zoom = 17;

            foreach (var coordinate in coordinates)
            {
                string url = $"https://maps.googleapis.com/maps/api/staticmap?center={coordinate.Latitude.ToString(CultureInfo.InvariantCulture)},{coordinate.Longitude.ToString(CultureInfo.InvariantCulture)}&zoom={zoom}&size=500x500&maptype=satellite&key={apiKey}";
                var imageBytes = await httpClient.GetByteArrayAsync(url);

                var entry = zipArchive.CreateEntry($"Image_{counter}.png");
                using var entryStream = entry.Open();
                using var memoryStream = new MemoryStream(imageBytes);
                memoryStream.CopyTo(entryStream);
                counter++;
            }
        }

        private static List<Coordinate> ReadCoordinatesFromCsv(string csvFilePath)
        {
            using var reader = new StreamReader(csvFilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<Coordinate>();

            return records.ToList();
        }
    }
}
