using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateWebApp.Models;

public class PropertyImage
{
    public int Id { get; set; }

    public int PropertyId { get; set; }

    [ForeignKey(nameof(PropertyId))]
    public Property? Property { get; set; }

    [Required, MaxLength(300)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsMain { get; set; }

    public int Order { get; set; }
}