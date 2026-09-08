using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateWebApp.Data;
using RealEstateWebApp.Models;

namespace RealEstateWebApp.Controllers
{
    public class PropertyRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PropertyRequestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================================
        // CREATE (GET)
        // ============================================================
        public async Task<IActionResult> Create()
        {
            ViewBag.PropertyTypes = await _context.PropertyTypes.ToListAsync();
            ViewBag.Cities = await _context.Cities.ToListAsync();
            return View();
        }

        // ============================================================
        // CREATE (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropertyRequest model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            model.UserId = user.Id;
            model.CreatedAt = DateTime.UtcNow;
            model.IsActive = true;

            _context.PropertyRequests.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ تم نشر طلبك بنجاح!";
            return RedirectToAction("MyRequests");
        }

        // ============================================================
        // MY REQUESTS - ✅ محسّن
        // ============================================================
        public async Task<IActionResult> MyRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var requests = await _context.PropertyRequests
                .AsNoTracking()
                .Include(r => r.PropertyType)
                .Include(r => r.City)
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .Take(20)
                .ToListAsync();

            return View(requests);
        }

        // ============================================================
        // ALL REQUESTS - ✅ محسّن
        // ============================================================
        public async Task<IActionResult> All()
        {
            var requests = await _context.PropertyRequests
                .AsNoTracking()
                .Include(r => r.PropertyType)
                .Include(r => r.City)
                .Include(r => r.User)
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .Take(20)
                .ToListAsync();

            return View(requests);
        }

        // ============================================================
        // DELETE
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Json(new { success = false, message = "⚠️ يجب تسجيل الدخول أولاً" });

                var request = await _context.PropertyRequests.FindAsync(id);
                if (request == null)
                    return Json(new { success = false, message = "⚠️ الطلب غير موجود" });

                if (request.UserId != user.Id)
                    return Json(new { success = false, message = "⚠️ لا يمكنك حذف طلب ليس ملكك" });

                _context.PropertyRequests.Remove(request);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "✅ تم حذف الطلب بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "❌ حدث خطأ: " + ex.Message });
            }
        }

        // ============================================================
        // EDIT (GET)
        // ============================================================
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var request = await _context.PropertyRequests
                .Include(r => r.PropertyType)
                .Include(r => r.City)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();
            if (request.UserId != user.Id) return Forbid();

            ViewBag.PropertyTypes = await _context.PropertyTypes.ToListAsync();
            ViewBag.Cities = await _context.Cities.ToListAsync();

            return View(request);
        }

        // ============================================================
        // EDIT (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PropertyRequest model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Json(new { success = false, message = "⚠️ يجب تسجيل الدخول أولاً" });

                if (id != model.Id)
                    return Json(new { success = false, message = "⚠️ طلب غير صحيح" });

                var request = await _context.PropertyRequests.FindAsync(id);
                if (request == null)
                    return Json(new { success = false, message = "⚠️ الطلب غير موجود" });

                if (request.UserId != user.Id)
                    return Json(new { success = false, message = "⚠️ لا يمكنك تعديل طلب ليس ملكك" });

                // تحديث البيانات
                request.Title = model.Title ?? request.Title;
                request.PropertyTypeId = model.PropertyTypeId;
                request.Status = model.Status ?? request.Status;
                request.CityId = model.CityId;
                request.Neighborhood = model.Neighborhood;
                request.MinPrice = model.MinPrice;
                request.MaxPrice = model.MaxPrice;
                request.MinArea = model.MinArea;
                request.MaxArea = model.MaxArea;
                request.MinRooms = model.MinRooms;
                request.MaxRooms = model.MaxRooms;
                request.PhoneNumber = model.PhoneNumber ?? request.PhoneNumber;
                request.Email = model.Email ?? request.Email;
                request.Description = model.Description;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "✅ تم تحديث الطلب بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "❌ حدث خطأ: " + ex.Message });
            }
        }

        // ============================================================
        // GET LATEST (API)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetLatest()
        {
            var requests = await _context.PropertyRequests
                .AsNoTracking()
                .Include(r => r.PropertyType)
                .Include(r => r.City)
                .Include(r => r.User)
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .Take(30)
                .Select(r => new
                {
                    r.Title,
                    PropertyType = r.PropertyType != null ? r.PropertyType.NameAr : "",
                    City = r.City != null ? r.City.NameAr : "",
                    UserName = r.User != null ? (r.User.FullName ?? r.User.UserName) : "",
                    r.CreatedAt,
                    r.Status,
                    r.MinPrice,
                    r.MaxPrice,
                    r.MinArea,
                    r.MaxArea,
                    r.Neighborhood,
                    r.Description
                })
                .ToListAsync();

            return Json(requests);
        }


        // ============================================================
        // DETAILS - عرض تفاصيل طلب معين (للبائع)
        // ============================================================
        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.PropertyRequests
                .Include(r => r.PropertyType)
                .Include(r => r.City)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            // التأكد من أن المستخدم هو صاحب العقار الذي أدى到这个 الطلب (أو أي مستخدم مسجل)
            // ولكن يمكن لأي مستخدم مسجل الدخول لرؤية تفاصيل الطلب
            return View(request);
        }
    }
}