using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    public class PaymentReceipt
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "رقم السند")]
        public string ReceiptNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "العميل")]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,3)")]
        [Display(Name = "المبلغ")]
        public decimal Amount { get; set; }
        [Display(Name = "اسم البنك")]
        public string? BankName { get; set; }

        [Display(Name = "السيارة المرتبطة")]
        public int? CarId { get; set; }

        [ForeignKey("CarId")]
        public Car? Car { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        [Display(Name = "خصم مسموح به")]
        public decimal Discount { get; set; } = 0;

        [NotMapped]
        [Display(Name = "المبلغ الإجمالي المخصوم من الدين")]
        public decimal TotalDeducted => Amount + Discount;

        [Required]
        [Display(Name = "تاريخ الاستلام")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "طريقة الدفع")]
        public string PaymentMethod { get; set; } = "نقداً";

        [Display(Name = "الرقم المرجعي")]
        public string? ReferenceNumber { get; set; }

        [Display(Name = "المرفقات")]
        public string? AttachmentPath { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }
    }
}