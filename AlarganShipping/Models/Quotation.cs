using System;
using System.ComponentModel.DataAnnotations;

namespace AlarganShipping.Models
{
    // نموذج عروض الأسعار (المبيعات / CRM)
    public class Quotation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "رقم عرض السعر")]
        public string QuoteNumber { get; set; }

        [Required]
        [Display(Name = "اسم العميل المحتمل")]
        public string ProspectName { get; set; }

        [Display(Name = "رقم هاتف العميل المحتمل")]
        public string ProspectPhone { get; set; }

        [Display(Name = "نوع وتفاصيل السيارة")]
        public string VehicleDetails { get; set; } // مثال: Range Rover 2024

        [Display(Name = "من (الموقع)")]
        public string FromLocation { get; set; }

        [Display(Name = "إلى (الوجهة)")]
        public string ToLocation { get; set; }

        [Display(Name = "التكلفة التقديرية (الإجمالي)")]
        public decimal EstimatedTotalCost { get; set; }

        [Display(Name = "تاريخ الإصدار")]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Display(Name = "صالح حتى (Valid Until)")]
        public DateTime ValidUntilDate { get; set; }

        [Display(Name = "حالة العرض")]
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected, Expired

        // إذا وافق العميل وتم تسجيله، يتم ربطه برقم العميل الفعلي لاحقاً
        public int? CustomerId { get; set; }
    }
}