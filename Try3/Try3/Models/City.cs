using System.ComponentModel.DataAnnotations;

namespace RealEstateWebApp.Models;

public class City
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string NameAr { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NameEn { get; set; }

   
    public double Latitude { get; set; }   // خط العرض
    public double Longitude { get; set; }  // خط الطول
    // =============================

    public ICollection<Neighborhood> Neighborhoods { get; set; } = new List<Neighborhood>();
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}