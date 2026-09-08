using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateWebApp.Models
{
    [Table("ClusterCenters")]
    public class ClusterCenter
    {
        public int Id { get; set; }

        public int CityId { get; set; }

        public int ClusterId { get; set; }

        public double AvgPrice { get; set; }

        public double AvgArea { get; set; }

        public double AvgRooms { get; set; }

        public double AvgBathrooms { get; set; }

        // ✅ هاتان الخاصيتان كانتا مفقودتين
        public double AvgPropertyTypeId { get; set; }

        public double AvgNeighborhoodEnc { get; set; }

        [ForeignKey("CityId")]
        public City? City { get; set; }
    }
}