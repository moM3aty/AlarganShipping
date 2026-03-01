using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    // نموذج الإشعارات (لإرسال تنبيهات للنظام الداخلي أو بوابة العميل)
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "عنوان الإشعار")]
        public string Title { get; set; } // مثال: فاتورة جديدة، تحديث حالة سيارة

        [Required]
        [Display(Name = "محتوى الإشعار")]
        public string Message { get; set; }

        [Display(Name = "تاريخ الإشعار")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "هل تم القراءة؟")]
        public bool IsRead { get; set; } = false;

        [Display(Name = "نوع الإشعار")]
        public string Type { get; set; } // SystemAlert, InvoiceAlert, TrackingUpdate

        // يمكن توجيه الإشعار لمستخدم نظام (موظف)
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        // أو يمكن توجيهه لعميل (ليظهر في تطبيقه أو بوابة التتبع)
        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }
    }
}