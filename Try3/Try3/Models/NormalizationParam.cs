using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateWebApp.Models
{
    [Table("NormalizationParams")]
    public class NormalizationParam
    {
        public int Id { get; set; }

        public int CityId { get; set; }

        [MaxLength(50)]
        public string FeatureName { get; set; } = string.Empty;

        public double MinValue { get; set; }

        public double MaxValue { get; set; }

        [ForeignKey("CityId")]
        public City? City { get; set; }
    }
}