using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RealEstateWebApp.Models
{
      [Table("SimilarProperties")]
        public class SimilarProperty
        {
       
            public int Id { get; set; }

            [Column("property_code")]
            public string PropertyCode { get; set; } = string.Empty;

            [Column("similar_property_code")]
            public string SimilarPropertyCode { get; set; } = string.Empty;

            [Column("similarity_score")]
            public double SimilarityScore { get; set; }

            [Column("rank_order")]
            public int RankOrder { get; set; }
        }
}
