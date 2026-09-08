using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateWebApp.Models
{
    public class PropertyRequest
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public int PropertyTypeId { get; set; }
        public PropertyType? PropertyType { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = string.Empty; // شراء / إيجار

        [Required]
        public int CityId { get; set; }
        public City? City { get; set; }

        [MaxLength(100)]
        public string? Neighborhood { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinArea { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxArea { get; set; }

        public byte? MinRooms { get; set; }
        public byte? MaxRooms { get; set; }

        public string? Description { get; set; }

        // ====== ✅ الخصائص الجديدة ======
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }
        // =============================

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
    }
}