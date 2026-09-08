using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateWebApp.Models
{
    [Table("PropertiesSample")]
    public class PropertySample
    {

        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required, MaxLength(20)]
        public string PriceCurrency { get; set; } = "USD";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Area { get; set; }

        public byte Rooms { get; set; }
        public byte Bathrooms { get; set; }
        public short? Floor { get; set; }

        public int PropertyTypeId { get; set; }
        public PropertyType? PropertyType { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        public int CityId { get; set; }
        public City? City { get; set; }

        public string Neighborhood { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Address { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string? Description { get; set; }

        // ====== ������� ������� ������� ======
        public string? DetailedLocation { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public string? AdvertiserPhone { get; set; }
        // ====================================

        [Required]
        public string AdvertiserId { get; set; } = string.Empty;
        public ApplicationUser? Advertiser { get; set; }

        public bool IsActive { get; set; } = true;
        public int ViewsCount { get; set; }

        public int? ClusterId { get; set; }
        public Cluster? Cluster { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<Feature> Features { get; set; } = new List<Feature>();
    }
}
