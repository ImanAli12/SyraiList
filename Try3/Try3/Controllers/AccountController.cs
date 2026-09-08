using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using RealEstateWebApp.Models;
using System.Linq; // ✅ أضفنا هذا لاستخدام All()

namespace RealEstateWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,
                                  SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return Json(new { success = false, message = "الرجاء إدخال البريد الإلكتروني وكلمة المرور" });

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Json(new { success = false, message = "البريد الإلكتروني غير مسجل" });

            // ✅ التحقق من حالة الحظر (LockoutEnd)
            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now)
            {
                return Json(new { success = false, message = "⚠️ هذا الحساب محظور. يرجى التواصل مع الإدارة." });
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
            if (result.Succeeded)
            {
                string redirectUrl = user.IsAdmin ? Url.Action("Index", "Dashboard") : Url.Action("Index", "Home");
                return Json(new { success = true, message = "تم تسجيل الدخول بنجاح", redirectUrl = redirectUrl });
            }

            return Json(new { success = false, message = "كلمة المرور غير صحيحة" });
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] IFormCollection form)
        {
            try
            {
                // اقرأ القيم يدوياً من الـ form
                string fullName = form["fullName"].ToString();
                string email = form["email"].ToString();
                string password = form["password"].ToString();
                string confirmPassword = form["confirmPassword"].ToString();
                string phoneNumber = form["phoneNumber"].ToString();

                // 🛑 اطبع القيمة في الـ Console (شوفيها في Output)
                Console.WriteLine($"📞 phoneNumber from form: '{phoneNumber}'");

                // التحقق من الحقول المطلوبة
                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    return Json(new { success = false, message = "الرجاء ملء جميع الحقول" });

                if (password != confirmPassword)
                    return Json(new { success = false, message = "كلمتا المرور غير متطابقتين" });

                if (password.Length < 8)
                    return Json(new { success = false, message = "كلمة المرور يجب أن تكون 8 أحرف على الأقل" });

                if (string.IsNullOrEmpty(phoneNumber))
                    return Json(new { success = false, message = "رقم الهاتف مطلوب" });

                // ✅ التحقق من صيغة البريد الإلكتروني (يحتوي على @ وينتهي بـ .com)
                if (!email.Contains("@") || !email.EndsWith(".com", System.StringComparison.OrdinalIgnoreCase))
                    return Json(new { success = false, message = "البريد الإلكتروني يجب أن يحتوي على @ وينتهي بـ .com" });

                // ✅ التحقق من رقم الهاتف (10 أرقام فقط)
                if (phoneNumber.Length != 10 || !phoneNumber.All(char.IsDigit))
                    return Json(new { success = false, message = "رقم الهاتف يجب أن يكون 10 أرقام (مثال: 0912345678)" });

                // التحقق من عدم تكرار البريد الإلكتروني
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                    return Json(new { success = false, message = "البريد الإلكتروني مسجل بالفعل" });

                // إنشاء المستخدم
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    PhoneNumber = phoneNumber
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Json(new { success = true, message = "✅ تم إنشاء الحساب بنجاح" });
                }

                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = errors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"❌ خطأ: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Json(new { success = true });
        }
    }
}