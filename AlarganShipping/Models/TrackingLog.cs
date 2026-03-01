using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    // نموذج سجل التتبع للعملاء (مكتمل 100%)
    public class TrackingLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "عنوان الحدث")]
        public string Title { get; set; } // مثال: تم الشراء، في الميناء، أبحرت

        [Display(Name = "الموقع")]
        public string Location { get; set; }

        [Display(Name = "تاريخ التحديث")]
        public DateTime UpdateDate { get; set; } = DateTime.Now;

        [Display(Name = "تفاصيل إضافية")]
        public string Description { get; set; }

        [Display(Name = "نسبة الإنجاز %")]
        public int ProgressPercentage { get; set; } // يستخدم لـ Progress Bar

        // العلاقات: هذا التحديث يخص سيارة معينة
        [Required]
        public int CarId { get; set; }
        [ForeignKey("CarId")]
        public virtual Car Car { get; set; }
    }
}