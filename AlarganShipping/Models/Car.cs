using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    // نموذج السيارة المطور ليشمل حساب الأرباح والبيانات المتقدمة
    public class Car
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "رقم الشاصي مطلوب")]
        [Display(Name = "رقم الشاصي (VIN)")]
        public string VIN { get; set; }

        [Required]
        [Display(Name = "الشركة المصنعة")]
        public string Make { get; set; }

        [Required]
        [Display(Name = "الموديل")]
        public string Model { get; set; }

        [Display(Name = "سنة الصنع")]
        public int Year { get; set; }

        [Display(Name = "اللون")]
        public string Color { get; set; }

        [Display(Name = "سعر الشراء ($)")]
        public decimal PurchasePrice { get; set; }

        // --- إضافات Premium ---

        [Display(Name = "صافي الربح التقديري")]
        public decimal EstimatedProfit { get; set; } = 0;

        [Display(Name = "كود السيارة للنظام")]
        public string InternalCode { get; set; } // مثال: AUTO-2024-001

        [Display(Name = "رقم المزاد")]
        public string LotNumber { get; set; }

        // الحالة (1-8 كما صممنا سابقاً)
        [Display(Name = "حالة السيارة")]
        public int StatusId { get; set; }

        // العلاقات
        [Required]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        public int? AuctionId { get; set; }
        [ForeignKey("AuctionId")]
        public virtual Auction Auction { get; set; }

        public int? ShipmentId { get; set; }
        [ForeignKey("ShipmentId")]
        public virtual Shipment Shipment { get; set; }

        public virtual ICollection<CarInspection> Inspections { get; set; }
        public virtual ICollection<TrackingLog> TrackingLogs { get; set; }
        public virtual ICollection<DocumentAttachment> Documents { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}