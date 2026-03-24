using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "رقم الفاتورة مطلوب")]
        [Display(Name = "رقم الفاتورة")]
        public string InvoiceNumber { get; set; }

        [Display(Name = "تاريخ الإصدار")]
        public DateTime IssueDate { get; set; }

        [Required(ErrorMessage = "يرجى اختيار العميل")]
        [Display(Name = "العميل")]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Required(ErrorMessage = "يرجى اختيار السيارة")]
        [Display(Name = "السيارة")]
        public int CarId { get; set; }
        public Car? Car { get; set; }

        [Display(Name = "فاتورة شحن وجمارك فقط (بدون شراء)")]
        public bool IsShippingOnly { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "قيمة السيارة")]
        public decimal CarPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "رسوم المزاد")]
        public decimal AuctionFees { get; set; }

        // --- الحقول الأصلية (لضمان عمل النظام القديم والتقارير) ---
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "النقل الداخلي")]
        public decimal LandFreight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "الشحن البحري")]
        public decimal SeaFreight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "الجمارك والضرائب")]
        public decimal CustomsFees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "عمولة الشركة")]
        public decimal AdminFees { get; set; }

        // --- أسماء مستعارة (للتوافق مع الشاشات الجديدة بدون كسر الداتابيز) ---
        [NotMapped]
        public decimal InlandTowing
        {
            get => LandFreight;
            set => LandFreight = value;
        }

        [NotMapped]
        public decimal CustomsAndTaxes
        {
            get => CustomsFees;
            set => CustomsFees = value;
        }

        [NotMapped]
        public decimal CompanyCommission
        {
            get => AdminFees;
            set => AdminFees = value;
        }

        // --- الحقول الجديدة المضافة ---
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "غرامات تأخير / أرضيات (Storage)")]
        public decimal StorageFees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "المبلغ المدفوع مقدماً")]
        public decimal AmountPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "الإجمالي")]
        public decimal TotalAmount { get; set; }

        // حقل محسوب لا يتم تخزينه في الداتابيز وإنما يُحسب وقت العرض
        [NotMapped]
        [Display(Name = "المبلغ المتبقي")]
        public decimal RemainingAmount => TotalAmount - AmountPaid;

        // الحقل الجديد لحل المشكلة
        [Display(Name = "ملاحظات الإلغاء أو التعديل")]
        public string? Notes { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "رسوم التحويل")]
        public decimal TransferFees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "نقل داخل عمان")]
        public decimal OmanTowingFees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "رسوم حجم السيارة")]
        public decimal CarSizeFees { get; set; }
    }
}