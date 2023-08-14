using CsvHelper.Configuration.Attributes;

namespace MLModel1_WebApi1.Models
{
    public class Coordinate
    {
        [Name("latitude")]
        public double Latitude { get; set; }

        [Name("longitude")]
        public double Longitude { get; set; }
    }
}
