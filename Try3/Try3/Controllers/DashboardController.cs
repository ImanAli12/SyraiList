
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateWebApp.Data;
using RealEstateWebApp.Models;
using RealEstateWebApp.ViewModels;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RealEstateWebApp.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DashboardController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ============================================================
        // الصفحة الرئيسية للداشبورد (Index)
        // ============================================================
        public async Task<IActionResult> Index()
        {
            var user = await _context.Users.FindAsync(GetCurrentUserId());
            if (user == null || !user.IsAdmin)
                return RedirectToAction("Index", "Home");

            // 🧠 التحقق: هل هو مدير محافظة (لديه محافظة محددة) أم مدير عام؟
            bool isGovernorateAdmin = !string.IsNullOrEmpty(user.Governorate);

            // ============================================================
            // 🟦 الحالة 1: مدير محافظة → يرى فقط العقارات المعلقة في محافظته
            // ============================================================
            if (isGovernorateAdmin)
            {
                string governorateName = user.Governorate;

                // جلب العقارات المعلقة التي تنتمي إلى محافظته فقط
                var pendingProperties = await _context.Properties
                    .Where(p => p.IsPending)
                    .Include(p => p.City)
                    .Include(p => p.PropertyType)
                    .Include(p => p.Advertiser)
                    .Where(p => p.City != null && p.City.NameAr == governorateName)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                // إنشاء ViewModel مبسط يحتوي فقط على العقارات المعلقة
                var model = new DashboardViewModel
                {
                    PendingProperties = pendingProperties,
                    PendingCount = pendingProperties.Count,
                    // نجعل باقي الخصائص فارغة أو صفراً (لن تُعرض في الـ View)
                    TotalUsers = 0,
                    TotalProperties = 0,
                    SoldProperties = 0,
                    Users = new List<ApplicationUser>()
                };

                // تمرير بيانات إضافية للـ View لمعرفة أن هذا مدير محافظة
                ViewBag.IsGovernorateAdmin = true;
                ViewBag.GovernorateName = governorateName;

                return View(model);
            }

            // ============================================================
            // 🟨 الحالة 2: مدير عام (بدون محافظة) → يرى كل شيء
            // ============================================================
            else
            {
                // 📊 1. توزيع العقارات حسب المدينة (مع تضخيم)
                var activeProperties = await _context.Properties
                    .Where(p => p.IsActive && !p.IsPending)
                    .Include(p => p.City)
                    .ToListAsync();

                var cityGroups = activeProperties
                    .GroupBy(p => p.City?.NameAr ?? "غير محدد")
                    .Select(g => new { City = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .Take(30)
                    .ToList();

                int realTotal = cityGroups.Sum(g => g.Count);
                double targetTotal = 27300.0;
                double factor = realTotal > 0 ? targetTotal / realTotal : 1;

                var inflatedData = cityGroups
                    .Select(g => (City: g.City, Count: (int)Math.Round(g.Count * factor)))
                    .ToList();

                int inflatedSum = inflatedData.Sum(g => g.Count);
                int diff = (int)targetTotal - inflatedSum;
                if (diff != 0 && inflatedData.Any())
                {
                    int maxIndex = inflatedData.FindIndex(g => g.Count == inflatedData.Max(x => x.Count));
                    var item = inflatedData[maxIndex];
                    inflatedData[maxIndex] = (item.City, item.Count + diff);
                }

                ViewBag.CityLabels = inflatedData.Select(g => g.City).ToArray();
                ViewBag.CityData = inflatedData.Select(g => g.Count).ToArray();

                // 📊 2. توزيع العقارات حسب مجموعات K-Means (مع تضخيم ديناميكي ليصبح المجموع ≈ 27,000)
                var clusterGroups = await _context.Properties
                    .Where(p => p.IsActive && !p.IsPending && p.ClusterId.HasValue)
                    .GroupBy(p => p.ClusterId.Value)
                    .Select(g => new { ClusterId = g.Key, Count = g.Count() })
                    .OrderBy(g => g.ClusterId)
                    .ToListAsync();

                var clusterCounts = new int[3];
                foreach (var g in clusterGroups)
                {
                    if (g.ClusterId >= 0 && g.ClusterId < 3)
                        clusterCounts[g.ClusterId] = g.Count;
                }

                // ✅ تضخيم الأرقام لجعل المجموع ≈ 27,000 (باستخدام أسماء متغيرات مختلفة لتجنب التعارض)
                int actualClusterTotal = clusterCounts.Sum();
                int targetClusterTotal = 27000;
                double clusterInflationFactor = actualClusterTotal > 0 ? (double)targetClusterTotal / actualClusterTotal : 1;

                var inflatedClusterCounts = clusterCounts
                    .Select(c => (int)Math.Round(c * clusterInflationFactor))
                    .ToArray();

                // ضبط الفروق الناتجة عن التقريب لضمان أن المجموع = 27000 بالضبط
                int inflatedClusterSum = inflatedClusterCounts.Sum();
                int clusterDiff = targetClusterTotal - inflatedClusterSum;
                if (clusterDiff != 0 && inflatedClusterCounts.Length > 0)
                {
                    int maxIndex = Array.IndexOf(inflatedClusterCounts, inflatedClusterCounts.Max());
                    inflatedClusterCounts[maxIndex] += clusterDiff;
                }

                ViewBag.ClusterLabels = new string[] { "اقتصادية", "متوسطة", "فاخرة" };
                ViewBag.ClusterData = inflatedClusterCounts; // القيم المضخمة للعرض
                                                             // (يمكنك حفظ القيم الأصلية في ViewBag.ClusterDataOriginal إذا أردت)

                // 📊 3. توزيع درجات التشابه (Similarity Scores)
                var similarityScores = await _context.SimilarProperties
                    .Select(sp => sp.SimilarityScore)
                    .ToListAsync();

                var bins = new int[5];
                foreach (var score in similarityScores)
                {
                    if (score < 0.2) bins[0]++;
                    else if (score < 0.4) bins[1]++;
                    else if (score < 0.6) bins[2]++;
                    else if (score < 0.8) bins[3]++;
                    else bins[4]++;
                }
                ViewBag.SimilarityLabels = new string[] { "0-20%", "20-40%", "40-60%", "60-80%", "80-100%" };
                ViewBag.SimilarityData = bins;

                // باقي الإحصائيات
                var totalProperties = await _context.Properties.CountAsync(p => p.IsActive && !p.IsPending);
                var pendingProperties = await _context.Properties
                    .Where(p => p.IsPending)
                    .Include(p => p.City)
                    .Include(p => p.PropertyType)
                    .Include(p => p.Advertiser)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var soldCountAll = await _context.Properties.CountAsync(p => p.IsSold && !p.IsPending);

                // ✅ تعريف model
                var model = new DashboardViewModel
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    TotalProperties = totalProperties,
                    SoldProperties = soldCountAll,
                    PendingCount = pendingProperties.Count,
                    Users = await _context.Users.ToListAsync(),
                    PendingProperties = pendingProperties
                };

                ViewBag.IsGovernorateAdmin = false;
                return View(model);
            }
        }

        // ============================================================
        // ✅ تبديل حالة المستخدم (حظر / تنشيط) - مُصحح
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "معرف المستخدم غير صحيح" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "المستخدم غير موجود" });

            var currentUser = await _context.Users.FindAsync(GetCurrentUserId());
            if (user.IsAdmin && currentUser?.Id != user.Id)
                return Json(new { success = false, message = "لا يمكن حظر مدير آخر" });

            user.LockoutEnd = user.LockoutEnd == null ? System.DateTimeOffset.MaxValue : null;
            await _context.SaveChangesAsync();

            var status = user.LockoutEnd != null ? "محظور" : "نشط";
            return Json(new { success = true, message = $"تم تغيير الحالة إلى {status}" });
        }

        // ============================================================
        // ✅ ترقية المستخدم إلى مدير - مُصحح
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(string userId, string governorate)
        {
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "معرف المستخدم غير صحيح" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "المستخدم غير موجود" });

            var currentUser = await _context.Users.FindAsync(GetCurrentUserId());
            if (currentUser?.Id == user.Id)
                return Json(new { success = false, message = "لا يمكن تغيير دورك الخاص" });

            if (string.IsNullOrEmpty(governorate))
                return Json(new { success = false, message = "الرجاء اختيار محافظة" });

            user.IsAdmin = true;
            user.Governorate = governorate;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"✅ تم ترقية المستخدم إلى مدير لمحافظة {governorate}" });
        }

        // ============================================================
        // ✅ تنزيل المدير إلى مستخدم عادي - مُصحح
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DemoteFromAdmin(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "معرف المستخدم غير صحيح" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "المستخدم غير موجود" });

            var currentUser = await _context.Users.FindAsync(GetCurrentUserId());
            if (currentUser?.Id == user.Id)
                return Json(new { success = false, message = "لا يمكن تغيير دورك الخاص" });

            if (!user.IsAdmin)
                return Json(new { success = false, message = "المستخدم ليس مديراً" });

            user.IsAdmin = false;
            user.Governorate = null;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "✅ تم تنزيل المستخدم إلى مستخدم عادي" });
        }

        // ============================================================
        // الموافقة على عقار
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveProperty(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null)
                return Json(new { success = false, message = "العقار غير موجود" });

            property.IsPending = false;
            property.IsActive = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"✅ تم نشر العقار '{property.Title}' بنجاح" });
        }

        // ============================================================
        // رفض عقار
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectProperty(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
                return Json(new { success = false, message = "العقار غير موجود" });

            var title = property.Title;
            foreach (var img in property.Images)
                DeleteImage(img.ImageUrl);

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"❌ تم رفض وحذف العقار '{title}'" });
        }

        // ============================================================
        // تدريب الخوارزميات
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TrainAlgorithm()
        {
            try
            {
                string pythonScriptsPath = @"C:\Users\iman ali\Desktop\توتة\Try3\Try3\PythonScripts";
                string pythonPath = @"C:\Users\iman ali\AppData\Local\Programs\Python\Python312\python.exe";
                string clusterScript = Path.Combine(pythonScriptsPath, "cluster_data.py");
                string similarityScript = Path.Combine(pythonScriptsPath, "GenerateSimilarities.py");

                if (!System.IO.File.Exists(pythonPath))
                    return Json(new { success = false, message = $"❌ Python غير موجود: {pythonPath}" });
                if (!System.IO.File.Exists(clusterScript))
                    return Json(new { success = false, message = $"❌ cluster_data.py غير موجود: {clusterScript}" });
                if (!System.IO.File.Exists(similarityScript))
                    return Json(new { success = false, message = $"❌ GenerateSimilarities.py غير موجود: {similarityScript}" });

                var output = new StringBuilder();
                var error = new StringBuilder();

                var startInfo = new ProcessStartInfo
                {
                    FileName = pythonPath,
                    Arguments = $"\"{clusterScript}\"",
                    WorkingDirectory = pythonScriptsPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding = System.Text.Encoding.UTF8
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string stdOutput = await process.StandardOutput.ReadToEndAsync();
                    string stdError = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    if (!string.IsNullOrEmpty(stdError))
                        return Json(new { success = false, message = $"❌ خطأ في K-Means:\n{stdError}" });
                    output.Append(stdOutput);
                }

                startInfo.Arguments = $"\"{similarityScript}\"";
                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string stdOutput = await process.StandardOutput.ReadToEndAsync();
                    string stdError = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    if (!string.IsNullOrEmpty(stdError))
                        return Json(new { success = false, message = $"❌ خطأ في Similarity:\n{stdError}" });
                    output.Append(stdOutput);
                }

                return Json(new
                {
                    success = true,
                    message = "✅ تم تدريب الخوارزميتين بنجاح!\n✅ K-Means: تم تجميع العقارات.\n✅ Similarity: تم حساب العقارات المشابهة."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"❌ خطأ: {ex.Message}" });
            }
        }

        // ============================================================
        // مساعدة: حذف الصورة
        // ============================================================
        private void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;
            string fileName = Path.GetFileName(imageUrl);
            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "image", "aqar", fileName);
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }

        // ============================================================
        // مساعدة: جلب ID المستخدم الحالي (string)
        // ============================================================
        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}








