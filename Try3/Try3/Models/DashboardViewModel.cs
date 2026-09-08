using RealEstateWebApp.Models;
using System.Collections.Generic;

namespace RealEstateWebApp.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProperties { get; set; }        // المنشورة (IsActive = true)
        public int SoldProperties { get; set; }         // إذا كان لديك عمود Status أو تبغي تحسبها بطريقة أخرى
        public int PendingCount { get; set; }           // المعلقة (IsActive = false)

        public List<ApplicationUser> Users { get; set; }
        public List<Property> PendingProperties { get; set; }
    }
}