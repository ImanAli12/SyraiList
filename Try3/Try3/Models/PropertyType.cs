using System.ComponentModel.DataAnnotations;

namespace RealEstateWebApp.Models;

public class PropertyType
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string NameAr { get; set; } = string.Empty;

    public ICollection<Property> Properties { get; set; } = new List<Property>();
}