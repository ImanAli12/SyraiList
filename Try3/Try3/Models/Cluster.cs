using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateWebApp.Models;

public class Cluster
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string NameAr { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal AvgPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AvgArea { get; set; }

    public byte AvgRooms { get; set; }

    public ICollection<Property> Properties { get; set; } = new List<Property>();
}