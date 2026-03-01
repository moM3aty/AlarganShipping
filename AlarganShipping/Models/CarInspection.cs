using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    // نموذج فحص السيارات لحماية الشركة من المطالبات (مكتمل 100%)
    public class CarInspection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "مرحلة الفحص")]
        public string InspectionStage { get; set; } // AtAuction, AtWarehouse, AtArrival

        [Display(Name = "تاريخ الفحص")]
        public DateTime InspectionDate { get; set; } = DateTime.Now;

        [Display(Name = "اسم الفاحص / السائق")]
        public string InspectorName { get; set; }

        [Display(Name = "هل يوجد مفتاح؟")]
        public bool HasKeys { get; set; }

        [Display(Name = "هل الملكية (Title) موجودة؟")]
        public bool HasTitle { get; set; }

        [Display(Name = "حالة التشغيل (Run & Drive)")]
        public bool IsRunAndDrive { get; set; }

        [Display(Name = "تفاصيل الأضرار والملاحظات")]
        public string DamageNotes { get; set; }

        // العلاقات: الفحص يتبع سيارة معينة
        [Required]
        public int CarId { get; set; }
        [ForeignKey("CarId")]
        public virtual Car Car { get; set; }
    }
}