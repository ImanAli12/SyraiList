using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RealEstateWebApp.Models;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RealEstateWebApp.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // ============================================================
            // ✅ بيانات حساب المدير
            // ============================================================
            string adminEmail = "admin@syrelis.com";
            string adminPassword = "Admin@123";
            string adminPhone = "0912345678";
            string adminName = "مدير النظام";

            // ============================================================
            // 1️⃣ دور Admin
            // ============================================================
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
                Console.WriteLine("✅ تم إنشاء دور Admin");
            }

            // ============================================================
            // 2️⃣ حساب المدير
            // ============================================================
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PhoneNumber = adminPhone,
                    PhoneNumberConfirmed = true,
                    IsAdmin = true,
                    FullName = adminName
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine($"✅ تم إنشاء حساب المدير: {adminEmail}");
                }
                else
                {
                    foreach (var error in result.Errors)
                        Console.WriteLine($"❌ خطأ في إنشاء المدير: {error.Description}");
                }
            }
            else
            {
                if (!adminUser.IsAdmin)
                {
                    adminUser.IsAdmin = true;
                    await userManager.UpdateAsync(adminUser);
                    Console.WriteLine("✅ تم ترقية المستخدم إلى مدير.");
                }

                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                Console.WriteLine($"ℹ️ حساب المدير موجود بالفعل: {adminEmail}");
            }

            // ============================================================
            // 3️⃣ أنواع العقارات: شقة، فيلا، مكتب، أرض
            // ============================================================
            var propertyTypes = new[] { "شقة", "فيلا", "مكتب", "أرض" };

            foreach (var typeName in propertyTypes)
            {
                var exists = await context.PropertyTypes.AnyAsync(t => t.NameAr == typeName);
                if (!exists)
                {
                    context.PropertyTypes.Add(new PropertyType { NameAr = typeName });
                    Console.WriteLine($"✅ تم إضافة نوع العقار: {typeName}");
                }
            }
            await context.SaveChangesAsync();

            // ============================================================
            // 4️⃣ المدن السورية (بنفس ترتيب JavaScript بالضبط)
            // ============================================================
            var cities = new[]
            {
                new { NameAr = "حمص",       Lat = 34.7327, Lng = 36.7154 },
                new { NameAr = "حلب",       Lat = 36.2025, Lng = 37.1343 },
                new { NameAr = "ريف دمشق",  Lat = 33.5000, Lng = 36.3000 },
                new { NameAr = "القنيطرة",  Lat = 33.1167, Lng = 35.8167 },
                new { NameAr = "الرقة",     Lat = 35.9510, Lng = 39.0100 },
                new { NameAr = "حماة",      Lat = 35.1320, Lng = 36.7480 },
                new { NameAr = "إدلب",      Lat = 35.9300, Lng = 36.6330 },
                new { NameAr = "درعا",      Lat = 32.6180, Lng = 36.1050 },
                new { NameAr = "السويداء",  Lat = 32.7060, Lng = 36.5690 },
                new { NameAr = "طرطوس",     Lat = 34.8870, Lng = 35.8860 },
                new { NameAr = "الحسكة",    Lat = 36.5050, Lng = 40.7440 },
                new { NameAr = "اللاذقية",  Lat = 35.5240, Lng = 35.7880 },
                new { NameAr = "دير الزور", Lat = 35.3330, Lng = 40.1470 },
                new { NameAr = "دمشق",      Lat = 33.5138, Lng = 36.2765 }
            };

            // ⚠️ إذا كانت المدن موجودة مسبقًا بترتيب مختلف، احذفها أولاً
            if (await context.Cities.AnyAsync())
            {
                context.Neighborhoods.RemoveRange(context.Neighborhoods);
                context.Cities.RemoveRange(context.Cities);
                await context.SaveChangesAsync();
                Console.WriteLine("🗑️ تم حذف المدن القديمة لإعادة التعبئة بالترتيب الصحيح");
            }

            // إدخال المدن بنفس الترتيب (سيحصل كل منها على Id تسلسلي 1..14)
            foreach (var c in cities)
            {
                context.Cities.Add(new City
                {
                    NameAr = c.NameAr,
                    Latitude = c.Lat,
                    Longitude = c.Lng
                });
            }

            await context.SaveChangesAsync();
            Console.WriteLine("✅ تم إدخال 14 مدينة سورية بالترتيب الصحيح");
        }
    }
}