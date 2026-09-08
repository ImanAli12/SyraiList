using RealEstateWebApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RealEstateWebApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<ClusterCenter> ClusterCenters { get; set; }
    public DbSet<City> Cities => Set<City>();
    public DbSet<Neighborhood> Neighborhoods => Set<Neighborhood>();

    public DbSet<SimilarProperty> SimilarProperties { get; set; }
    public DbSet<PropertyType> PropertyTypes => Set<PropertyType>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Cluster> Clusters => Set<Cluster>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<PropertyRequest> PropertyRequests => Set<PropertyRequest>();
    public DbSet<NormalizationParam> NormalizationParams { get; set; }
    public DbSet<NeighborhoodEncoding> NeighborhoodEncodings { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Cluster>().Property(c => c.Id).ValueGeneratedNever();

        builder.Entity<Favorite>().HasKey(f => new { f.UserId, f.PropertyId });

        builder.Entity<Property>()
            .HasOne(p => p.Advertiser)
            .WithMany(u => u.Properties)
            .HasForeignKey(p => p.AdvertiserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Property>()
            .HasOne(p => p.Cluster)
            .WithMany(c => c.Properties)
            .HasForeignKey(p => p.ClusterId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Property>()
            .HasOne(p => p.City)
            .WithMany(c => c.Properties)
            .HasForeignKey(p => p.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        // ❌ تم حذف علاقة Neighborhood لأن Neighborhood أصبح string

        builder.Entity<Property>()
            .HasOne(p => p.PropertyType)
            .WithMany(t => t.Properties)
            .HasForeignKey(p => p.PropertyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PropertyImage>()
            .HasOne(pi => pi.Property)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Favorite>()
            .HasOne(f => f.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Favorite>()
            .HasOne(f => f.Property)
            .WithMany(p => p.Favorites)
            .HasForeignKey(f => f.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Neighborhood>()
            .HasOne(n => n.City)
            .WithMany(c => c.Neighborhoods)
            .HasForeignKey(n => n.CityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}