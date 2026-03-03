using System.ComponentModel.DataAnnotations;

namespace AlarganShipping.Models
{
    // جدول لأشكال السيارات
    public class VehicleShape
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } // مثال: صالون، دفع رباعي، بيك أب
    }

    // جدول لأنواع الضرائب
    public class TaxType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } // مثال: ضريبة قيمة مضافة، ضريبة جمركية
    }

    // جدول لطرق الضريبة
    public class TaxMethod
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } // مثال: نسبة مئوية (%)، مبلغ مقطوع ($)
    }
}