using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AlarganShipping.Models
{
    // نموذج شركات النقل الداخلي (الريكفري) في أمريكا (مكتمل 100%)
    public class Transporter
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم شركة النقل / السائق مطلوب")]
        [Display(Name = "اسم الناقل")]
        public string Name { get; set; }

        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; }

        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Display(Name = "رقم الترخيص (US DOT Number)")]
        public string DotNumber { get; set; }

        [Display(Name = "بيانات التأمين")]
        public string InsuranceDetails { get; set; }

        [Display(Name = "تاريخ انتهاء التأمين")]
        public DateTime? InsuranceExpiryDate { get; set; }

        [Display(Name = "تقييم الناقل (من 1 إلى 5)")]
        public int Rating { get; set; } = 5;

        // العلاقات: أوامر النقل التي تم تكليف هذه الشركة بها
        public virtual ICollection<DispatchOrder> DispatchOrders { get; set; }
    }
}