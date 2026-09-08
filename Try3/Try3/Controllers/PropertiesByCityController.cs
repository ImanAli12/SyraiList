using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateWebApp.Data;
using RealEstateWebApp.Models;

namespace RealEstateWebApp.Controllers
{
    [Route("PropertiesByCity")]
    public class PropertiesByCityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PropertiesByCityController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // عرض العقارات في مدينة معينة - ✅ محسّن
        // ============================================================
        [Route("City/{cityName}")]
        public async Task<IActionResult> City(string cityName)
        {
            if (string.IsNullOrEmpty(cityName))
                return RedirectToAction("Index", "Home");

            // جلب جميع العقارات النشطة في هذه المدينة (مع الصورة الرئيسية والمجموعة)
            var properties = await _context.Properties
                .AsNoTracking()
                .Include(p => p.City)
                .Include(p => p.PropertyType)
                .Include(p => p.Advertiser)
                .Where(p => p.City != null && p.City.NameAr == cityName && p.IsActive && !p.IsPending)
                .OrderByDescending(p => p.CreatedAt)
                .Take(100) // يمكنك زيادة العدد حسب الحاجة
                .Select(p => new Property
                {
                    Id = p.Id,
                    Code = p.Code,
                    Title = p.Title,
                    Price = p.Price,
                    PriceCurrency = p.PriceCurrency,
                    Area = p.Area,
                    Rooms = p.Rooms,
                    Bathrooms = p.Bathrooms,
                    Floor = p.Floor,
                    Status = p.Status,
                    City = p.City,
                    PropertyType = p.PropertyType,
                    Neighborhood = p.Neighborhood,
                    CreatedAt = p.CreatedAt,
                    IsActive = p.IsActive,
                    IsSold = p.IsSold,
                    IsPending = p.IsPending,
                    ClusterId = p.ClusterId,    // ✅ المفتاح: رقم المجموعة (0,1,2)
                    Advertiser = p.Advertiser,
                    Images = p.Images.Where(i => i.IsMain).Take(1).ToList()
                })
                .ToListAsync();

            var city = await _context.Cities
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.NameAr == cityName);

            ViewBag.CityName = city?.NameAr ?? cityName;
            ViewBag.PropertyCount = properties.Count;

            return View(properties);
        }
    }
}