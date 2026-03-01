using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AlarganShipping.Models
{
    // نموذج المخلصين الجمركيين (مكتمل 100%)
    public class ClearingAgent
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم شركة التخليص مطلوب")]
        [Display(Name = "اسم شركة التخليص / الوكيل")]
        public string Name { get; set; }

        [Display(Name = "رقم التواصل")]
        public string Phone { get; set; }

        [Display(Name = "الميناء المعتمد لديه")]
        public string PortLocation { get; set; } // مثال: ميناء صلالة

        [Display(Name = "نسبة العمولة أو الرسوم المقطوعة")]
        public decimal StandardFee { get; set; }

        [Display(Name = "رقم الترخيص الجمركي")]
        public string LicenseNumber { get; set; }

        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        // العلاقات: قد يتم ربط المخلص بشحنات معينة مستقبلاً
    }
}