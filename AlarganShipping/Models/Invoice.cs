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
        [Range(0, 99999999.99, ErrorMessage = "القيمة يجب أن تكون 0 أو أكبر")]
        public decimal CarPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "رسوم المزاد")]
        [Range(0, 99999999.99, ErrorMessage = "القيمة يجب أن تكون 0 أو أكبر")]
        public decimal AuctionFees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "النقل الداخلي")]
        [Range(0, 99999999.99, ErrorMessage = "القيمة يجب أن تكون 0 أو أكبر")]
        public decimal LandFreight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "الشحن البحري")]
        [Range(0, 99999999.99, ErrorMessage = "القيمة يجب أن تكون 0 أو أكبر")]
        public decimal SeaFreight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "رسوم التخليص فقط")]
        [Range(0, 99999999.99, ErrorMessage = "القيمة يجب أن تكون 0 أو أكبر")]
        public decimal CustomsFees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "عمولة الشركة")]
        [Range(0, 99999999.99, ErrorMessage = "القيمة يجب أن تكون 0 أو أكبر")]
        public decimal AdminFees { get; set; }

        [NotMapped]
        public decimal InlandTowing
        {
            get => LandFreight;
            set => LandFreight = value;
        }

        [NotMapped]
        [Display(Name = "رسوم التخليص فقط")]
        public decimal CustomsAndTaxes
        {
            get => CustomsFees;
            set => CustomsFees = value;
        }

        [NotMapped]
        public decimal ClearanceFees
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

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "غرامات تأخير / أرضيات (Storage)")]
        [Range(0, 99999999.99, ErrorMessage = "القيمة يجب أن تكون 0 أو أكبر")]
        public decimal StorageFees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "المبلغ المدفوع مقدماً")]
        [Range(0, 99999999.99, ErrorMessage = "القيمة يجب أن تكون 0 أو أكبر")]
        public decimal AmountPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "الإجمالي")]
        public decimal TotalAmount { get; set; }

        [NotMapped]
        [Display(Name = "المبلغ المتبقي")]
        public decimal RemainingAmount => TotalAmount - AmountPaid;

        [Display(Name = "ملاحظات الإلغاء أو التعديل")]
        public string? Notes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "رسوم التحويل")]
        [Range(0, 99999999.99, ErrorMessage = "القيمة يجب أن تكون 0 أو أكبر")]
        public decimal TransferFees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "نقل داخل عمان")]
        [Range(0, 99999999.99, ErrorMessage = "القيمة يجب أن تكون 0 أو أكبر")]
        public decimal OmanTowingFees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "رسوم حجم السيارة")]
        [Range(0, 99999999.99, ErrorMessage = "القيمة يجب أن تكون 0 أو أكبر")]
        public decimal CarSizeFees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "مبلغ الضريبة")]
        [Range(0, 99999999.99, ErrorMessage = "القيمة يجب أن تكون 0 أو أكبر")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "مبلغ الجمارك")]
        [Range(0, 99999999.99, ErrorMessage = "القيمة يجب أن تكون 0 أو أكبر")]
        public decimal CustomsAmount { get; set; }
    }
}