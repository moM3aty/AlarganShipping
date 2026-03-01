using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    // نموذج سندات القبض والدفعات المالية (مكتمل 100%)
    public class PaymentReceipt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "رقم السند")]
        public string ReceiptNumber { get; set; }

        [Required(ErrorMessage = "المبلغ مطلوب")]
        [Display(Name = "المبلغ المدفوع")]
        public decimal Amount { get; set; }

        [Display(Name = "تاريخ الدفع")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Display(Name = "طريقة الدفع")]
        public string PaymentMethod { get; set; } // Cash, Bank Transfer, Cheque

        [Display(Name = "الرقم المرجعي / رقم الشيك")]
        public string ReferenceNumber { get; set; }

        [Display(Name = "ملاحظات")]
        public string Notes { get; set; }

        // العلاقات: السند يتم دفعه من قبل عميل
        [Required]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
    }
}