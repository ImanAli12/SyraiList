using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateWebApp.Models;

public class Favorite
{
    public string UserId { get; set; } = string.Empty;

    public int PropertyId { get; set; }

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    [ForeignKey(nameof(PropertyId))]
    public Property? Property { get; set; }
}