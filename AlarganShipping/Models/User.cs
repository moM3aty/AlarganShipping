using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AlarganShipping.Models
{
    // نموذج مستخدمي النظام (الإدارة والموظفين) مكتمل 100%
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد غير صحيحة")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [Display(Name = "كلمة المرور")]
        public string PasswordHash { get; set; }

        [Display(Name = "الصلاحية")]
        public string Role { get; set; } = "Admin"; // Admin, Accountant, Operations

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "حساب نشط")]
        public bool IsActive { get; set; } = true;

        // العلاقات: الإشعارات الموجهة لهذا المستخدم
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}