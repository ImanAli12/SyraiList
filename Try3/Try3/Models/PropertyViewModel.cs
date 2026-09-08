using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RealEstateWebApp.Models
{
    public class PropertyViewModel
    {
        public int Id { get; set; }
        // الخطوة 1: الوصف
        [Required(ErrorMessage = "نوع العقار مطلوب")]
        public int PropertyTypeId { get; set; }

        [Required(ErrorMessage = "طبيعة العقار مطلوبة")]
        public string Status { get; set; } = "للبيع";

        [Required(ErrorMessage = "السعر مطلوب")]
        [Range(0, double.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من أو يساوي 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "العملة مطلوبة")]
        public string PriceCurrency { get; set; } = "USD";

        [Required(ErrorMessage = "العنوان مطلوب")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        // الخطوة 2: الوسائط
        public IFormFile? MainImage { get; set; }
        public List<IFormFile>? AdditionalImages { get; set; } = new();
        public string? MainImageUrl { get; set; }
        public List<string>? AdditionalImageUrls { get; set; } = new();

        // الخطوة 3: الموقع
        [Required(ErrorMessage = "المدينة مطلوبة")]
        public int CityId { get; set; }

        [Required(ErrorMessage = "الحي مطلوب")]
        [MaxLength(100)]
        public string Neighborhood { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Address { get; set; }

        [Required(ErrorMessage = "الموقع التفصيلي مطلوب")]
        public string DetailedLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "يرجى تحديد الموقع على الخريطة")]
        public double? Latitude { get; set; }

        [Required(ErrorMessage = "يرجى تحديد الموقع على الخريطة")]
        public double? Longitude { get; set; }

        // الخطوة 4: التفاصيل
        [Required(ErrorMessage = "رقم المحضر مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "رقم المحضر يجب أن يكون أكبر من 0")]
        public int PropertyNumber { get; set; }

        [Required(ErrorMessage = "المساحة مطلوبة")]
        [Range(1, double.MaxValue, ErrorMessage = "المساحة يجب أن تكون أكبر من 0")]
        public decimal Area { get; set; }

        public byte? Rooms { get; set; }
        public byte? Bathrooms { get; set; }
        public short? Floor { get; set; }
        public byte? TotalFloors { get; set; }

        [Required(ErrorMessage = "تاريخ الاتاحة مطلوب")]
        public DateTime AvailableFrom { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "رقم الهاتف يجب أن يبدأ بـ 09 ويتكون من 10 أرقام")]
        public string AdvertiserPhone { get; set; } = string.Empty;

        // الخطوة 5: المرافق
        public List<int>? FeatureIds { get; set; } = new();

        // بيانات إضافية للـ View
        public List<PropertyType>? PropertyTypes { get; set; }
        public List<City>? Cities { get; set; }
        public List<Feature>? Features { get; set; }
    }
}