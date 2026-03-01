using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    // نموذج تسعير الخدمات (لحساب التكاليف تلقائياً في الحاسبة والفواتير)
    public class ServiceRate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "نوع الخدمة")]
        public string ServiceType { get; set; } // LandFreight (نقل داخلي), SeaFreight (شحن بحري), Customs (تخليص)

        [Display(Name = "حجم / نوع السيارة")]
        public string VehicleType { get; set; } // Sedan, SUV, Pickup, Heavy Machinery

        [Display(Name = "السعر ($)")]
        [Column(TypeName = "decimal(18,2)")] // تحديد النوع والدقة في قاعدة البيانات
        public decimal Price { get; set; }

        // تسعير المسارات (من وإلى)
        [Display(Name = "موقع الانطلاق (من)")]
        public int? OriginLocationId { get; set; }
        [ForeignKey("OriginLocationId")]
        public virtual Location OriginLocation { get; set; }

        [Display(Name = "موقع الوصول (إلى)")]
        public int? DestinationLocationId { get; set; }
        [ForeignKey("DestinationLocationId")]
        public virtual Location DestinationLocation { get; set; }

        [Display(Name = "ملاحظات التسعير")]
        public string Notes { get; set; } // مثال: "شامل رسوم العبور"
    }
}