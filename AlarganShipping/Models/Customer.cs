using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AlarganShipping.Models
{
    // نموذج العملاء المطور ليشمل بيانات دخول البوابة الذكية
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم العميل مطلوب")]
        [Display(Name = "اسم العميل")]
        public string Name { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; }

        [EmailAddress]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Display(Name = "الرقم المدني / الهوية")]
        public string CivilId { get; set; }

        [Display(Name = "العنوان")]
        public string Address { get; set; }

        [Display(Name = "إجمالي المستحقات (متبقي)")]
        public decimal TotalBalance { get; set; } = 0;

        [Display(Name = "إجمالي المدفوع")]
        public decimal TotalPaid { get; set; } = 0;

        // --- إضافات Premium (بيانات بوابة العميل) ---

        [Display(Name = "اسم مستخدم البوابة")]
        public string PortalUsername { get; set; }

        [Display(Name = "كلمة مرور البوابة")]
        public string PortalPassword { get; set; }

        [Display(Name = "البوابة نشطة")]
        public bool IsPortalActive { get; set; } = true;

        [Display(Name = "كود العميل")]
        public string CustomerCode { get; set; } // مثال: CUST-001

        // العلاقات الشاملة
        public virtual ICollection<Car> Cars { get; set; }
        public virtual ICollection<PaymentReceipt> PaymentReceipts { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
        public virtual ICollection<DocumentAttachment> Documents { get; set; }
    }
}