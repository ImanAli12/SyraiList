using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RealEstateWebApp.Models;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // ✅ مهم لاستخدام AnyAsync

namespace RealEstateWebApp.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // ✅ الوصول إلى قاعدة البيانات
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // ============================================================
            // ✅ بيانات حساب المدير (عدّل حسب رغبتك)
            // ============================================================
            string adminEmail = "admin@syrelis.com";
            string adminPassword = "Admin@123";
            string adminPhone = "0912345678";
            string adminName = "مدير النظام";

            // ============================================================
            // 1️⃣ التأكد من وجود دور "Admin"
            // ============================================================
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
                Console.WriteLine("✅ تم إنشاء دور Admin");
            }

            // ============================================================
            // 2️⃣ البحث عن المستخدم
            // ============================================================
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // ============================================================
                // إنشاء مستخدم جديد
                // ============================================================
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
                    // ============================================================
                    // إضافة المستخدم إلى دور Admin
                    // ============================================================
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine($"✅ تم إنشاء حساب المدير: {adminEmail}");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"❌ خطأ في إنشاء المدير: {error.Description}");
                    }
                }
            }
            else
            {
                // ============================================================
                // إذا كان المستخدم موجوداً، تأكد من أنه مدير
                // ============================================================
                if (!adminUser.IsAdmin)
                {
                    adminUser.IsAdmin = true;
                    await userManager.UpdateAsync(adminUser);
                    Console.WriteLine("✅ تم ترقية المستخدم إلى مدير.");
                }

                // تأكد من أنه في دور Admin
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }

                Console.WriteLine($"ℹ️ حساب المدير موجود بالفعل: {adminEmail}");
            }

            // ============================================================
            // 3️⃣ تعبئة أنواع العقارات: شقة، فيلا، مكتب، أرض
            // ============================================================
            var propertyTypes = new[] { "شقة", "فيلا", "مكتب", "أرض" };

            foreach (var typeName in propertyTypes)
            {
                var exists = await context.PropertyTypes
                    .AnyAsync(t => t.NameAr == typeName);

                if (!exists)
                {
                    context.PropertyTypes.Add(new PropertyType
                    {
                        NameAr = typeName
                    });

                    Console.WriteLine($"✅ تم إضافة نوع العقار: {typeName}");
                }
            }

            await context.SaveChangesAsync();
        }
    }
}