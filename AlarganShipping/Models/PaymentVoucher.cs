using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    public class PaymentVoucher
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "رقم سند الصرف")]
        public string VoucherNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ السند مطلوب")]
        [Display(Name = "تاريخ السند")]
        public DateTime VoucherDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "المبلغ مطلوب")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "المبلغ المصروف")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "اسم المستفيد مطلوب")]
        [Display(Name = "يصرف للسيد / الجهة (المستفيد)")]
        public string BeneficiaryName { get; set; } = string.Empty;

        // اختياري: في حال كان الصرف (إرجاع أموال) لعميل مسجل لدينا
        [Display(Name = "ارتباط بعميل (اختياري)")]
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Display(Name = "طريقة الدفع")]
        public string PaymentMethod { get; set; } = "تحويل بنكي"; // Cash, Bank Transfer, Check

        [Display(Name = "رقم المرجع / الشيك")]
        public string? ReferenceNumber { get; set; }

        [Required(ErrorMessage = "البيان مطلوب")]
        [Display(Name = "وذلك عن (البيان)")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "تصنيف المصروف")]
        public string Category { get; set; } = "مشتريات سيارات"; // مزادات، شحن، تشغيلية، إرجاع لعميل
    }
}