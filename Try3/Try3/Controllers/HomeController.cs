using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateWebApp.Data;
using RealEstateWebApp.Models;
using System.Threading.Tasks;

namespace RealEstateWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================================
        // الصفحة الرئيسية - ✅ مع دعم تجاوز التوجيه للمدير
        // ============================================================
        public async Task<IActionResult> Index(bool? skipRedirect = false)
        {
            // ✅ إذا كان المستخدم مسجلاً دخوله ولم يطلب تجاوز التوجيه
            if (User.Identity.IsAuthenticated && skipRedirect != true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null && user.IsAdmin)
                {
                    // ✅ إعادة توجيه المدير إلى لوحة التحكم
                    return RedirectToAction("Index", "Dashboard");
                }
            }

            // ✅ جلب 12 مدينة فقط للعرض (للمستخدمين العاديين أو للمدير عند تجاوز التوجيه)
            var cities = await _context.Cities
                .AsNoTracking()
                .OrderBy(c => c.NameAr)
                .Take(30)
                .ToListAsync();

            return View(cities);
        }

        // ============================================================
        // CONTACT (POST)
        // ============================================================
        [HttpPost]
        public IActionResult Contact(string name, string email, string message)
        {
            // هنا يمكنك حفظ الرسالة في قاعدة البيانات لاحقاً
            return Json(new { success = true, message = "تم إرسال رسالتك بنجاح" });
        }
    }
}