using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateWebApp.Models
{
    [Table("NeighborhoodEncodings")]
    public class NeighborhoodEncoding
    {
        public int Id { get; set; }

        public int CityId { get; set; }

        [MaxLength(100)]
        public string NeighborhoodName { get; set; } = string.Empty;

        public int EncodedValue { get; set; }

        [ForeignKey("CityId")]
        public City? City { get; set; }
    }
}