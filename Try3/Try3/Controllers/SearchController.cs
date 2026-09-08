using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateWebApp.Data;
using RealEstateWebApp.Models;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateWebApp.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Cities = await _context.Cities.OrderBy(c => c.NameAr).ToListAsync();
            ViewBag.PropertyTypes = await _context.PropertyTypes.OrderBy(p => p.NameAr).ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            int? propertyTypeId,
            string? status,
            int? cityId,
            string? neighborhood,
            decimal? minPrice,
            decimal? maxPrice,
            string? currency,
            decimal? minArea,
            decimal? maxArea,
            byte? rooms,
            byte? bathrooms,
            short? floor,
            short? totalFloors)
        {
            try
            {
                if (!cityId.HasValue || cityId.Value == 0)
                {
                    ViewBag.NoDataMessage = "الرجاء اختيار مدينة.";
                    return PartialView("_PropertyResults", new List<Property>());
                }

                // ==========================================================
                // هل أدخل المستخدم سعراً؟
                // ==========================================================
                bool hasPrice = minPrice.HasValue && maxPrice.HasValue && minPrice.Value > 0 && maxPrice.Value > 0;

                // ==========================================================
                // 1. الخوارزمية تحدد المجموعة
                // ==========================================================
                int? selectedCluster = null;
                string clusterMessage = "";

                var centers = await _context.ClusterCenters
                    .Where(c => c.CityId == cityId.Value)
                    .ToListAsync();

                double userPrice = 0, userArea = 0, userRooms = 0, userBathrooms = 0, userPropertyType = 0, userNeighborhood = 0;
                double normPrice = 0, normArea = 0, normRooms = 0, normBathrooms = 0, normType = 0, normNeighborhood = 0;
                Dictionary<string, NormalizationParam> normParams = new();

                double WEIGHT_PRICE = hasPrice ? 2.0 : 0.0;
                const double WEIGHT_AREA = 1.5;
                const double WEIGHT_ROOMS = 1.0;
                const double WEIGHT_BATHROOMS = 1.0;
                const double WEIGHT_PROPERTY_TYPE = 3.0;
                const double WEIGHT_NEIGHBORHOOD = 2.5;

                string cityName = await _context.Cities
                    .Where(c => c.Id == cityId.Value)
                    .Select(c => c.NameAr)
                    .FirstOrDefaultAsync() ?? "";

                // ==========================================================
                // 🧠 المنطق الذكي: إذا اختار المستخدم فيلا بدون سعر، اجعله فاخراً
                // ==========================================================
                string propertyTypeName = null;
                if (propertyTypeId.HasValue)
                {
                    propertyTypeName = await _context.PropertyTypes
                        .Where(pt => pt.Id == propertyTypeId.Value)
                        .Select(pt => pt.NameAr)
                        .FirstOrDefaultAsync();
                }

                if (!hasPrice && propertyTypeName != null &&
                    (propertyTypeName.Contains("فيلا") || propertyTypeName.Contains("ناعورة") || propertyTypeName.Contains("قصر")))
                {
                    selectedCluster = 2; // فاخرة
                   // clusterMessage = $"🔍 بما أنك تبحث عن {propertyTypeName} في مدينة {cityName} ولم تحدد سعراً، تم تصنيفك ضمن العقارات **الفاخرة**.";
                }
                else
                {
                    if (centers.Any())
                    {
                        normParams = await _context.NormalizationParams
                            .Where(n => n.CityId == cityId.Value)
                            .ToDictionaryAsync(n => n.FeatureName);

                        int? neighborhoodEnc = null;
                        if (!string.IsNullOrWhiteSpace(neighborhood))
                        {
                            var enc = await _context.NeighborhoodEncodings
                                .Where(e => e.CityId == cityId.Value && e.NeighborhoodName == neighborhood.Trim())
                                .Select(e => (int?)e.EncodedValue)
                                .FirstOrDefaultAsync();
                            if (enc.HasValue)
                                neighborhoodEnc = enc.Value;
                        }

                        userPrice = hasPrice ? (double)((minPrice.Value + maxPrice.Value) / 2) : 0;
                        userArea = (minArea.HasValue && maxArea.HasValue && minArea.Value > 0 && maxArea.Value > 0)
                            ? (double)((minArea.Value + maxArea.Value) / 2) : 0;
                        userRooms = rooms ?? 0;
                        userBathrooms = bathrooms ?? 0;
                        userPropertyType = propertyTypeId ?? 0;
                        userNeighborhood = neighborhoodEnc ?? 0;

                        normPrice = hasPrice ? Normalize(userPrice, normParams, "Price") : 0;
                        normArea = Normalize(userArea, normParams, "Area");
                        normRooms = Normalize(userRooms, normParams, "Rooms");
                        normBathrooms = Normalize(userBathrooms, normParams, "Bathrooms");
                        normType = Normalize(userPropertyType, normParams, "PropertyTypeId");
                        normNeighborhood = Normalize(userNeighborhood, normParams, "NeighborhoodEnc");

                        var normalizedCenters = centers.Select(c => new
                        {
                            Center = c,
                            NormPrice = hasPrice ? Normalize(c.AvgPrice, normParams, "Price") : 0,
                            NormArea = Normalize(c.AvgArea, normParams, "Area"),
                            NormRooms = Normalize(c.AvgRooms, normParams, "Rooms"),
                            NormBathrooms = Normalize(c.AvgBathrooms, normParams, "Bathrooms"),
                            NormType = Normalize(c.AvgPropertyTypeId, normParams, "PropertyTypeId"),
                            NormNeighborhood = Normalize(c.AvgNeighborhoodEnc, normParams, "NeighborhoodEnc")
                        }).ToList();

                        var closest = normalizedCenters
                            .Select(c => new
                            {
                                c.Center,
                                Distance = Math.Sqrt(
                                    Math.Pow((normPrice - c.NormPrice) * WEIGHT_PRICE, 2) +
                                    Math.Pow((normArea - c.NormArea) * WEIGHT_AREA, 2) +
                                    Math.Pow((normRooms - c.NormRooms) * WEIGHT_ROOMS, 2) +
                                    Math.Pow((normBathrooms - c.NormBathrooms) * WEIGHT_BATHROOMS, 2) +
                                    Math.Pow((normType - c.NormType) * WEIGHT_PROPERTY_TYPE, 2) +
                                    Math.Pow((normNeighborhood - c.NormNeighborhood) * WEIGHT_NEIGHBORHOOD, 2)
                                )
                            })
                            .OrderBy(x => x.Distance)
                            .FirstOrDefault();

                       //if (closest != null)
                        //{
                        //    selectedCluster = closest.Center.ClusterId;
                        //    string clusterName = selectedCluster switch
                        //    {
                        //        0 => "اقتصادية",
                        //        1 => "متوسطة",
                        //        2 => "فاخرة",
                        //        _ => "غير مصنفة"
                        //    };
                        //    clusterMessage = $"🔍 تم تصنيف تفضيلاتك ضمن العقارات **{clusterName}** في مدينة {cityName}.";
                        //    if (!hasPrice)
                        //        clusterMessage += " (تم تجاهل السعر لأنك لم تحدده، وتم الاعتماد على باقي التفضيلات)";
                        //}
                    }
                }

                // ==========================================================
                // 2. بناء الاستعلام
                // ==========================================================
                var baseQuery = _context.Properties
                    .AsNoTracking()
                    .Include(p => p.City)
                    .Include(p => p.PropertyType)
                    .Include(p => p.Images)
                    .Where(p => p.IsActive && p.CityId == cityId.Value);

                if (propertyTypeId.HasValue && propertyTypeId.Value > 0)
                    baseQuery = baseQuery.Where(p => p.PropertyTypeId == propertyTypeId.Value);
                if (!string.IsNullOrWhiteSpace(neighborhood))
                    baseQuery = baseQuery.Where(p => p.Neighborhood != null && p.Neighborhood.Contains(neighborhood.Trim()));
                if (minPrice.HasValue && minPrice.Value > 0)
                    baseQuery = baseQuery.Where(p => p.Price >= minPrice.Value);
                if (maxPrice.HasValue && maxPrice.Value > 0)
                    baseQuery = baseQuery.Where(p => p.Price <= maxPrice.Value);
                if (minArea.HasValue && minArea.Value > 0)
                    baseQuery = baseQuery.Where(p => p.Area >= minArea.Value);
                if (maxArea.HasValue && maxArea.Value > 0)
                    baseQuery = baseQuery.Where(p => p.Area <= maxArea.Value);
                if (rooms.HasValue && rooms.Value > 0)
                    baseQuery = baseQuery.Where(p => p.Rooms == rooms.Value);
                if (bathrooms.HasValue && bathrooms.Value > 0)
                    baseQuery = baseQuery.Where(p => p.Bathrooms == bathrooms.Value);
                if (floor.HasValue && floor.Value >= 0)
                    baseQuery = baseQuery.Where(p => p.Floor == floor.Value);

                var queryWithCluster = baseQuery;
                if (selectedCluster.HasValue)
                    queryWithCluster = queryWithCluster.Where(p => p.ClusterId == selectedCluster.Value);

                // ==========================================================
                // 3. جلب النتائج (مع التراجع الصامت)
                // ==========================================================
                var results = new List<Property>();
                var propertiesList = await queryWithCluster.ToListAsync();

                if (propertiesList.Any())
                {
                    results = propertiesList;
                }
                else
                {
                    propertiesList = await baseQuery.ToListAsync();
                    if (propertiesList.Any())
                    {
                        results = propertiesList;
                    }
                }

                if (!results.Any())
                {
                    ViewBag.NoDataMessage = "لا توجد عقارات تطابق معايير البحث.";
                    return PartialView("_PropertyResults", new List<Property>());
                }

                // ==========================================================
                // 4. ترتيب النتائج حسب التشابه
                // ==========================================================
                var orderedResults = results
                    .Select(p =>
                    {
                        int propNeighEnc = 0;
                        if (!string.IsNullOrWhiteSpace(p.Neighborhood))
                        {
                            var enc = _context.NeighborhoodEncodings
                                .Where(e => e.CityId == cityId.Value && e.NeighborhoodName == p.Neighborhood)
                                .Select(e => (int?)e.EncodedValue)
                                .FirstOrDefault();
                            if (enc.HasValue)
                                propNeighEnc = enc.Value;
                        }

                        double nPrice = hasPrice ? Normalize((double)p.Price, normParams, "Price") : 0;
                        double nArea = Normalize((double)p.Area, normParams, "Area");
                        double nRooms = Normalize(p.Rooms, normParams, "Rooms");
                        double nBath = Normalize(p.Bathrooms, normParams, "Bathrooms");
                        double nType = Normalize(p.PropertyTypeId, normParams, "PropertyTypeId");
                        double nNeigh = Normalize(propNeighEnc, normParams, "NeighborhoodEnc");

                        double distance = Math.Sqrt(
                            Math.Pow((normPrice - nPrice) * WEIGHT_PRICE, 2) +
                            Math.Pow((normArea - nArea) * WEIGHT_AREA, 2) +
                            Math.Pow((normRooms - nRooms) * WEIGHT_ROOMS, 2) +
                            Math.Pow((normBathrooms - nBath) * WEIGHT_BATHROOMS, 2) +
                            Math.Pow((normType - nType) * WEIGHT_PROPERTY_TYPE, 2) +
                            Math.Pow((normNeighborhood - nNeigh) * WEIGHT_NEIGHBORHOOD, 2)
                        );

                        return new { Property = p, Distance = distance };
                    })
                    .OrderBy(x => x.Distance)
                    .Take(30)
                    .Select(x => x.Property)
                    .ToList();

                ViewBag.ClusterMessage = clusterMessage;

                return PartialView("_PropertyResults", orderedResults);
            }
            catch (Exception ex)
            {
                ViewBag.NoDataMessage = "خطأ: " + ex.Message;
                return PartialView("_PropertyResults", new List<Property>());
            }
        }

        private double Normalize(double value, Dictionary<string, NormalizationParam> normParams, string featureName)
        {
            if (normParams.TryGetValue(featureName, out var param))
            {
                if (param.MaxValue > param.MinValue)
                    return (value - param.MinValue) / (param.MaxValue - param.MinValue);
            }
            return 0;
        }
    }
}