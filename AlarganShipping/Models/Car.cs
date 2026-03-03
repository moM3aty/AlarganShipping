using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "رقم الشاصي مطلوب")]
        [StringLength(17, MinimumLength = 17, ErrorMessage = "رقم الشاصي يجب أن يكون 17 رمزاً")]
        [Display(Name = "رقم الشاصي")]
        public string VIN { get; set; }

        [Required(ErrorMessage = "الماركة مطلوبة")]
        [Display(Name = "الماركة")]
        public string Make { get; set; }

        [Required(ErrorMessage = "الموديل مطلوب")]
        [Display(Name = "الموديل والنوع")]
        public string Model { get; set; }

        [Required(ErrorMessage = "سنة الصنع مطلوبة")]
        [Display(Name = "سنة الصنع")]
        public int Year { get; set; }

        [Required(ErrorMessage = "اللون مطلوب")]
        [Display(Name = "اللون")]
        public string Color { get; set; }

        // الكود الداخلي الذي كان يظهر فارغاً
        public string? InternalCode { get; set; }

        // الحقل الجديد الخاص بصورة السيارة
        [Display(Name = "صورة السيارة")]
        public string? MainImageUrl { get; set; }

        [Display(Name = "حالة المركبة")]
        public int StatusId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedProfit { get; set; }

        public string? LotNumber { get; set; }
        public DateTime? AddDate { get; set; }
        public string? PlateNo { get; set; }
        public string? CurrentLocation { get; set; }
        public string? CardNo { get; set; }
        public string? EngineNo { get; set; }
        public string? TaxType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TaxAmount { get; set; }
        public string? CustomsDeclaration { get; set; }
        public string? InsuranceNo { get; set; }
        public string? VehicleType { get; set; }
        public string? TaxMethod { get; set; }
        public string? VehicleShape { get; set; }

        [Required(ErrorMessage = "العميل مطلوب")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [Required(ErrorMessage = "المعرض/المزاد مطلوب")]
        public int AuctionId { get; set; }
        [ForeignKey("AuctionId")]
        public Auction? Auction { get; set; }

        public int? ShipmentId { get; set; }
        [ForeignKey("ShipmentId")]
        public Shipment? Shipment { get; set; }

        public ICollection<TrackingLog> TrackingLogs { get; set; } = new List<TrackingLog>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<DocumentAttachment> DocumentAttachments { get; set; } = new List<DocumentAttachment>();
        public ICollection<CarInspection> CarInspections { get; set; } = new List<CarInspection>();
    }
}