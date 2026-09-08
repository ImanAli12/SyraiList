using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateWebApp.Models;

public class Neighborhood
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string NameAr { get; set; } = string.Empty;

    public int CityId { get; set; }

    [ForeignKey(nameof(CityId))]
    public City? City { get; set; }

    public ICollection<Property> Properties { get; set; } = new List<Property>();
}