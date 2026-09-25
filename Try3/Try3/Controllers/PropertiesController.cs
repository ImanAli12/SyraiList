using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RealEstateWebApp.Data;
using RealEstateWebApp.Models;

namespace RealEstateWebApp.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PropertiesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // ============================================================
        // CREATE (GET)
        // ============================================================
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var viewModel = new PropertyViewModel
            {
                PropertyTypes = await _context.PropertyTypes.ToListAsync(),
                Cities = await _context.Cities.ToListAsync(),
                Features = await _context.Features.ToListAsync(),
                Status = "للبيع",
                PriceCurrency = "$",
                AvailableFrom = DateTime.Now
            };
            return View(viewModel);
        }

        // ============================================================
        // CREATE (POST) - ✅ معدل لحفظ الصور الإضافية بشكل صحيح
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropertyViewModel viewModel, IFormFile? MainImage)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var city = await _context.Cities.FindAsync(viewModel.CityId);
                if (city == null)
                {
                    ModelState.AddModelError("CityId", "المدينة المحددة غير موجودة");
                    return View(viewModel);
                }

                var property = new Property
                {
                    Code = GeneratePropertyCode(),
                    Title = viewModel.Title,
                    Price = viewModel.Price,
                    PriceCurrency = viewModel.PriceCurrency,
                    Area = viewModel.Area,
                    Rooms = viewModel.Rooms ?? 0,
                    Bathrooms = viewModel.Bathrooms ?? 0,
                    Floor = viewModel.Floor,
                    Status = viewModel.Status,
                    CityId = viewModel.CityId,
                    City = city,
                    Neighborhood = viewModel.Neighborhood ?? string.Empty,
                    Address = viewModel.Address,
                    Description = viewModel.Description,
                    Latitude = viewModel.Latitude,
                    Longitude = viewModel.Longitude,
                    PropertyTypeId = viewModel.PropertyTypeId,
                    AdvertiserId = user.Id,
                    IsActive = false,
                    IsPending = true,
                    IsSold = false,
                    CreatedAt = DateTime.UtcNow,
                    ViewsCount = 0,
                    DetailedLocation = viewModel.DetailedLocation,
                    AvailableFrom = viewModel.AvailableFrom,
                    AdvertiserPhone = viewModel.AdvertiserPhone
                };

                // 📸 حفظ الصورة الأساسية
                if (MainImage != null && MainImage.Length > 0)
                {
                    var mainImagePath = await SaveImageAsync(MainImage, "aqar");
                    property.Images.Add(new PropertyImage
                    {
                        ImageUrl = "/image/aqar/" + mainImagePath,
                        IsMain = true,
                        Order = 0
                    });
                }

                // 📸 ✅ حفظ الصور الإضافية (باستخدام Request.Form.Files)
                var additionalImages = Request.Form.Files.Where(f => f.Name == "AdditionalImages");
                if (additionalImages.Any())
                {
                    int order = 1;
                    foreach (var img in additionalImages)
                    {
                        if (img != null && img.Length > 0)
                        {
                            var imgPath = await SaveImageAsync(img, "aqar");
                            property.Images.Add(new PropertyImage
                            {
                                ImageUrl = "/image/aqar/" + imgPath,
                                IsMain = false,
                                Order = order++
                            });
                        }
                    }
                }

                // 🏷️ حفظ المرافق المختارة
                if (viewModel.FeatureIds != null && viewModel.FeatureIds.Any())
                {
                    var features = await _context.Features
                        .Where(f => viewModel.FeatureIds.Contains(f.Id))
                        .ToListAsync();
                    foreach (var feature in features)
                    {
                        property.Features.Add(feature);
                    }
                }

                _context.Properties.Add(property);
                await _context.SaveChangesAsync();

                TempData["Success"] = "📋 تم استلام طلبك! عم نراجع عقارك وسنرد عليك قريباً.";
                return RedirectToAction("MyProperties", "Properties");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "❌ خطأ: " + ex.Message;
                return View(viewModel);
            }
        }

        // ============================================================
        // ALL PROPERTIES (جميع العقارات المنشورة فقط)
        // ============================================================
        

        // ============================================================
        // MY PROPERTIES (عقاراتي - تشمل المعلقة والمنشورة)
        // ============================================================
        public async Task<IActionResult> MyProperties(int page = 1, int pageSize = 10)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var connectionString = _context.Database.GetConnectionString();
            var userId = user.Id;
            var offset = (page - 1) * pageSize;

            var pendingCount = await _context.Properties.CountAsync(p => p.AdvertiserId == userId && p.IsPending == true);
            var activeCount = await _context.Properties.CountAsync(p => p.AdvertiserId == userId && p.IsActive == true && (p.IsPending == false || p.IsPending == null));

            ViewBag.PendingCount = pendingCount;
            ViewBag.ActiveCount = activeCount;

            var countSql = "SELECT COUNT(*) FROM Properties WHERE AdvertiserId = @UserId";
            var totalCount = 0;
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(countSql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    totalCount = (int)await cmd.ExecuteScalarAsync();
                }
            }

            var sql = @"
        WITH PagedProperties AS (
            SELECT 
                p.Id, p.Code, p.Title, p.Price, p.PriceCurrency,
                p.Area, p.Rooms, p.Bathrooms, p.Floor, p.Status,
                p.Neighborhood, p.Description, p.CreatedAt, p.IsActive, p.IsSold, p.IsPending,
                c.NameAr AS CityName, c.Id AS CityId,
                pt.NameAr AS PropertyTypeName, pt.Id AS PropertyTypeId,
                (SELECT TOP 1 ImageUrl FROM PropertyImages WHERE PropertyId = p.Id AND IsMain = 1 ORDER BY [Order]) AS MainImageUrl,
                ROW_NUMBER() OVER (ORDER BY p.IsPending DESC, p.CreatedAt DESC) AS RowNum
            FROM Properties p
            LEFT JOIN Cities c ON p.CityId = c.Id
            LEFT JOIN PropertyTypes pt ON p.PropertyTypeId = pt.Id
            WHERE p.AdvertiserId = @UserId
        )
        SELECT *
        FROM PagedProperties
        WHERE RowNum BETWEEN @StartRow AND @EndRow
        ORDER BY RowNum";

            var properties = new List<Property>();
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@StartRow", offset + 1);
                    cmd.Parameters.AddWithValue("@EndRow", offset + pageSize);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var property = new Property
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Code = reader.GetString(reader.GetOrdinal("Code")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                PriceCurrency = reader.GetString(reader.GetOrdinal("PriceCurrency")),
                                Area = reader.GetDecimal(reader.GetOrdinal("Area")),
                                Rooms = reader.GetByte(reader.GetOrdinal("Rooms")),
                                Bathrooms = reader.GetByte(reader.GetOrdinal("Bathrooms")),
                                Floor = reader.IsDBNull(reader.GetOrdinal("Floor")) ? (short?)null : reader.GetInt16(reader.GetOrdinal("Floor")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                Neighborhood = reader.GetString(reader.GetOrdinal("Neighborhood")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                IsSold = reader.GetBoolean(reader.GetOrdinal("IsSold")),
                                IsPending = reader.GetBoolean(reader.GetOrdinal("IsPending")),
                                CityId = reader.GetInt32(reader.GetOrdinal("CityId")),
                                City = new City
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CityId")),
                                    NameAr = reader.GetString(reader.GetOrdinal("CityName"))
                                },
                                PropertyTypeId = reader.GetInt32(reader.GetOrdinal("PropertyTypeId")),
                                PropertyType = new PropertyType
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("PropertyTypeId")),
                                    NameAr = reader.GetString(reader.GetOrdinal("PropertyTypeName"))
                                },
                                Images = reader.IsDBNull(reader.GetOrdinal("MainImageUrl"))
                                    ? new List<PropertyImage>()
                                    : new List<PropertyImage> { new PropertyImage { ImageUrl = reader.GetString(reader.GetOrdinal("MainImageUrl")), IsMain = true } }
                            };
                            properties.Add(property);
                        }
                    }
                }
            }

            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(properties);
        }

        // ============================================================
        // DETAIL SEARCH (البحث عن عقار بواسطة الكود)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> DetailSearch(string code)
        {
            if (string.IsNullOrEmpty(code)) return NotFound();

            var property = await _context.Properties
                .Include(p => p.City)
                .Include(p => p.PropertyType)
                .Include(p => p.Images)
                .Include(p => p.Advertiser)
                .Include(p => p.Features)
                .FirstOrDefaultAsync(p => p.Code == code);

            if (property == null) return NotFound();

            var similarCodes = await _context.SimilarProperties
                .Where(sp => sp.PropertyCode == code)
                .OrderBy(sp => sp.RankOrder)
                .Select(sp => sp.SimilarPropertyCode)
                .Take(30)
                .ToListAsync();

            var similarProperties = await _context.Properties
                .Include(p => p.City)
                .Include(p => p.PropertyType)
                .Include(p => p.Images)
                .Include(p => p.Advertiser)
                .Where(p => similarCodes.Contains(p.Code) && p.IsActive && !p.IsPending)
                .ToListAsync();

            ViewBag.SimilarProperties = similarProperties;
            return View(property);
        }

        // ============================================================
        // EDIT (GET)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Features)
                .Include(p => p.City)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (property.AdvertiserId != user?.Id)
                return Forbid();

            // ✅ منع التعديل إذا كان مباعاً أو قيد المراجعة
            if (property.IsSold)
            {
                TempData["Error"] = "❌ لا يمكن تعديل عقار مباع.";
                return RedirectToAction("MyProperties");
            }


            int propertyNumber = 0;
            if (!string.IsNullOrEmpty(property.Code))
            {
                var parts = property.Code.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out int num))
                    propertyNumber = num;
            }

            var viewModel = new PropertyViewModel
            {
                Id = id,   // ✅ تأكد من تعيين الـ Id
                PropertyTypeId = property.PropertyTypeId,
                Status = property.Status,
                Price = property.Price,
                PriceCurrency = property.PriceCurrency,
                Title = property.Title,
                Description = property.Description,
                CityId = property.CityId,
                Neighborhood = property.Neighborhood ?? string.Empty,
                Address = property.Address,
                DetailedLocation = property.DetailedLocation ?? "",
                Latitude = property.Latitude,
                Longitude = property.Longitude,
                PropertyNumber = propertyNumber,
                Area = property.Area,
                Rooms = property.Rooms,
                Bathrooms = property.Bathrooms,
                Floor = property.Floor,
                TotalFloors = 0,
                AvailableFrom = property.AvailableFrom ?? DateTime.Now,
                AdvertiserPhone = property.AdvertiserPhone ?? "",
                MainImageUrl = property.Images?.FirstOrDefault(i => i.IsMain)?.ImageUrl,
                AdditionalImageUrls = property.Images?.Where(i => !i.IsMain).Select(i => i.ImageUrl).ToList() ?? new List<string>(),
                FeatureIds = property.Features?.Select(f => f.Id).ToList() ?? new List<int>(),
                PropertyTypes = await _context.PropertyTypes.ToListAsync(),
                Cities = await _context.Cities.ToListAsync(),
                Features = await _context.Features.ToListAsync()
            };

            return View(viewModel);
        }
        // ============================================================
        // DELETE
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (property.AdvertiserId != user?.Id) return Forbid();

            foreach (var img in property.Images) DeleteImage(img.ImageUrl);

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "تم حذف العقار بنجاح" });
        }

        // ============================================================
        // MARK AS SOLD
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsSold(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "⚠️ يجب تسجيل الدخول أولاً";
                    return RedirectToAction("MyProperties");
                }

                var property = await _context.Properties.FindAsync(id);
                if (property == null)
                {
                    TempData["Error"] = "⚠️ العقار غير موجود";
                    return RedirectToAction("MyProperties");
                }

                if (property.AdvertiserId != user.Id)
                {
                    TempData["Error"] = "⚠️ لا يمكنك تعديل هذا العقار";
                    return RedirectToAction("MyProperties");
                }

                if (property.IsPending == true)
                {
                    TempData["Error"] = "⚠️ هذا العقار قيد المراجعة، لا يمكن تغيير حالته حالياً.";
                    return RedirectToAction("MyProperties");
                }

                property.IsSold = !property.IsSold;
                property.IsActive = !property.IsSold;

                await _context.SaveChangesAsync();

                TempData["Success"] = property.IsSold ? " تم تحديد العقار كمباع" : "تم إلغاء تحديد المباع";
                return RedirectToAction("MyProperties");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ حدث خطأ: {ex.Message}";
                return RedirectToAction("MyProperties");
            }
        }

        // ============================================================
        // ADD TO FAVORITES
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> AddToFavorites(int propertyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "⚠️ يجب تسجيل الدخول أولاً" });

            var property = await _context.Properties.FindAsync(propertyId);
            if (property == null)
                return Json(new { success = false, message = "⚠️ العقار غير موجود" });

            if (property.IsPending == true)
                return Json(new { success = false, message = "⚠️ هذا العقار قيد المراجعة حالياً." });

            if (property.AdvertiserId == user.Id)
                return Json(new { success = false, message = "⚠️ لا يمكنك إضافة عقارك إلى المفضلة" });

            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.PropertyId == propertyId);

            if (existing != null)
                return Json(new { success = false, message = "⚠️ هذا العقار مضاف بالفعل إلى المفضلة" });

            _context.Favorites.Add(new Favorite
            {
                UserId = user.Id,
                PropertyId = propertyId,
                SavedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "✅ تم إضافة العقار إلى المفضلة بنجاح" });
        }

        // ============================================================
        // REMOVE FROM FAVORITES
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> RemoveFromFavorites(int propertyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "⚠️ يجب تسجيل الدخول أولاً" });

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.PropertyId == propertyId);

            if (favorite == null)
                return Json(new { success = false, message = "⚠️ العقار غير موجود في المفضلة" });

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "✅ تم إزالة العقار من المفضلة بنجاح" });
        }

        // ============================================================
        // MY FAVORITES
        // ============================================================
        public async Task<IActionResult> MyFavorites()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var favorites = await _context.Favorites
                .Include(f => f.Property)
                    .ThenInclude(p => p.City)
                .Include(f => f.Property)
                    .ThenInclude(p => p.Images)
                .Include(f => f.Property)
                    .ThenInclude(p => p.PropertyType)
                .Where(f => f.UserId == user.Id)
                .OrderByDescending(f => f.SavedAt)
                .ToListAsync();

            return View(favorites);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PropertyViewModel viewModel)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var property = await _context.Properties
                    .Include(p => p.Images)
                    .Include(p => p.Features)
                    .FirstOrDefaultAsync(p => p.Id == viewModel.Id);

                if (property == null) return NotFound();

                // ✅ التحقق من أن المستخدم هو المالك
                if (property.AdvertiserId != user.Id) return Forbid();

                // ✅ لا يمكن تعديل العقار إذا كان مباعاً
                if (property.IsSold)
                {
                    TempData["Error"] = "❌ لا يمكن تعديل عقار مباع.";
                    return RedirectToAction("MyProperties");
                }

                // ✅ لا يمكن تعديل العقار إذا كان قيد المراجعة (معلّق)
               

                // ✅ تحديث الحقول
                property.Title = viewModel.Title;
                property.Price = viewModel.Price;
                property.PriceCurrency = viewModel.PriceCurrency;
                property.Area = viewModel.Area;
                property.Rooms = viewModel.Rooms ?? 0;
                property.Bathrooms = viewModel.Bathrooms ?? 0;
                property.Floor = viewModel.Floor;
                property.Status = viewModel.Status;
                property.CityId = viewModel.CityId;
                property.Neighborhood = viewModel.Neighborhood ?? string.Empty;
                property.Address = viewModel.Address;
                property.Description = viewModel.Description;
                property.Latitude = viewModel.Latitude;
                property.Longitude = viewModel.Longitude;
                property.DetailedLocation = viewModel.DetailedLocation ?? string.Empty;
                property.AvailableFrom = viewModel.AvailableFrom;
                property.AdvertiserPhone = viewModel.AdvertiserPhone;
                property.PropertyTypeId = viewModel.PropertyTypeId;

                // ✅ تحديث المرافق
                property.Features.Clear();
                if (viewModel.FeatureIds != null && viewModel.FeatureIds.Any())
                {
                    var features = await _context.Features
                        .Where(f => viewModel.FeatureIds.Contains(f.Id))
                        .ToListAsync();
                    foreach (var feature in features)
                    {
                        property.Features.Add(feature);
                    }
                }

                // 🔥 **الأهم:** بعد التعديل، يصبح العقار قيد المراجعة مجدداً
                property.IsPending = true;
                property.IsActive = false;

                await _context.SaveChangesAsync();

                TempData["Success"] = "✅ تم تحديث العقار بنجاح! سيتم مراجعته من قبل الإدارة.";
                return RedirectToAction("MyProperties");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ حدث خطأ: {ex.Message}";
                // إعادة تحميل البيانات للـ View في حالة الخطأ
                viewModel.PropertyTypes = await _context.PropertyTypes.ToListAsync();
                viewModel.Cities = await _context.Cities.ToListAsync();
                viewModel.Features = await _context.Features.ToListAsync();
                return View(viewModel);
            }
        }
        // ============================================================
        // HELPER METHODS
        // ============================================================
        private async Task<string> SaveImageAsync(IFormFile file, string folder)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "image", folder);
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return uniqueFileName;
        }

        private void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            string fileName = Path.GetFileName(imageUrl);
            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "image", "aqar", fileName);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        private string GeneratePropertyCode()
        {
            var lastProperty = _context.Properties.OrderByDescending(p => p.Id).FirstOrDefault();
            int nextNumber = (lastProperty?.Id ?? 0) + 1;
            return $"PROP-{DateTime.Now.Year}-{nextNumber:D6}";
        }



        // ============================================================
        // إضافات جديدة: البحث عن أقرب طلب زبون مطابق للعقار
        // ============================================================

        /// <summary>
        /// البحث عن الطلب الأكثر تطابقاً مع عقار معين
        /// </summary>
        private PropertyRequest? FindBestMatchingRequest(int propertyId)
        {
            // جلب العقار المطلوب مع معلوماته الأساسية
            var property = _context.Properties
                .Include(p => p.City)
                .Include(p => p.PropertyType)
                .FirstOrDefault(p => p.Id == propertyId);

            if (property == null) return null;

            // جلب جميع الطلبات النشطة من نفس المدينة ونفس نوع العقار
            var requests = _context.PropertyRequests
                .Include(r => r.City)
                .Include(r => r.PropertyType)
                .Where(r => r.IsActive
                            && r.CityId == property.CityId
                            && r.PropertyTypeId == property.PropertyTypeId)
                .ToList();

            if (!requests.Any()) return null;

            // حساب درجة التطابق لكل طلب
            var scoredRequests = requests.Select(req =>
            {
                int score = 0;

                // 1. المدينة (تم التصفية مسبقاً) + 100
                score += 100;

                // 2. نوع العقار (تم التصفية مسبقاً) + 100
                score += 100;

                // 3. الحي (تطابق تام) + 50
                if (!string.IsNullOrEmpty(property.Neighborhood) &&
                    !string.IsNullOrEmpty(req.Neighborhood) &&
                    property.Neighborhood.Equals(req.Neighborhood, StringComparison.OrdinalIgnoreCase))
                {
                    score += 50;
                }

                // 4. السعر ضمن النطاق + 30
                if ((!req.MinPrice.HasValue || property.Price >= req.MinPrice.Value) &&
                    (!req.MaxPrice.HasValue || property.Price <= req.MaxPrice.Value))
                {
                    score += 30;
                }

                // 5. المساحة ضمن النطاق + 20
                if ((!req.MinArea.HasValue || property.Area >= req.MinArea.Value) &&
                    (!req.MaxArea.HasValue || property.Area <= req.MaxArea.Value))
                {
                    score += 20;
                }

                // 6. عدد الغرف ضمن النطاق + 10
                if ((!req.MinRooms.HasValue || property.Rooms >= req.MinRooms.Value) &&
                    (!req.MaxRooms.HasValue || property.Rooms <= req.MaxRooms.Value))
                {
                    score += 10;
                }

                return new { Request = req, Score = score };
            })
            .OrderByDescending(x => x.Score)
            .ToList();

            // نعيد الطلب الأعلى درجة، مع التأكد أن له درجة أكبر من 0
            var best = scoredRequests.FirstOrDefault(x => x.Score > 0);
            return best?.Request;
        }

        /// <summary>
        /// الدالة التي تستدعيها الواجهة (الزر في MyProperties)
        /// </summary>
        public IActionResult FindMatchingRequest(int id)
        {
            var request = FindBestMatchingRequest(id);
            if (request == null)
            {
                TempData["Error"] = "❌ لا يوجد طلب زبون مطابق لعقارك حالياً.";
                return RedirectToAction("MyProperties");
            }

            // التوجيه إلى صفحة تفاصيل الطلب
            return RedirectToAction("Details", "PropertyRequest", new { id = request.Id });
        }
    }
}