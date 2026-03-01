using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    // نموذج الفواتير والتكاليف الشاملة (مكتمل 100%)
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "رقم الفاتورة")]
        public string InvoiceNumber { get; set; }

        [Display(Name = "تاريخ الإصدار")]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        // تفاصيل التكاليف
        [Display(Name = "تكلفة السيارة (من المزاد)")]
        public decimal CarPrice { get; set; }

        [Display(Name = "رسوم المزاد")]
        public decimal AuctionFees { get; set; }

        [Display(Name = "النقل الداخلي (Towing)")]
        public decimal LandFreight { get; set; }

        [Display(Name = "الشحن البحري (Ocean Freight)")]
        public decimal SeaFreight { get; set; }

        [Display(Name = "التخليص الجمركي")]
        public decimal CustomsFees { get; set; }

        [Display(Name = "رسوم إدارية وعمولة")]
        public decimal AdminFees { get; set; }

        [Display(Name = "إجمالي الفاتورة")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "حالة الدفع")]
        public bool IsPaid { get; set; } = false;

        // العلاقات: الفاتورة تصدر لعميل معين وتخص سيارة معينة
        [Required]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        [Required]
        public int CarId { get; set; }
        [ForeignKey("CarId")]
        public virtual Car Car { get; set; }
    }
}