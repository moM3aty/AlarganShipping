using System;
using System.ComponentModel.DataAnnotations;

namespace AlarganShipping.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string PasswordHash { get; set; }

        public string Role { get; set; } = "Staff"; // Admin, Staff, Accountant
        public bool IsActive { get; set; } = true;

        // --- نظام الصلاحيات الديناميكي ---
        public bool CanManageCars { get; set; } = false;
        public bool CanManageCustomers { get; set; } = false;
        public bool CanManageFinance { get; set; } = false;
        public bool CanManageSettings { get; set; } = false;

        // --- بيانات صفحة اتصل بنا ---
        public bool ShowOnContactPage { get; set; } = false;
        public string? JobTitle { get; set; } // مثال: مدير قسم المزادات
        public string? PhoneNumber { get; set; }
        public string? DepartmentIcon { get; set; } // fa-gavel, fa-headset...

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}