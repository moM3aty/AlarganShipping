using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    // نموذج الأرشفة الإلكترونية والمرفقات (مكتمل 100%)
    public class DocumentAttachment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "نوع المستند")]
        public string DocumentType { get; set; } = string.Empty;

        [Display(Name = "وصف المستند")]
        public string? Description { get; set; } // الحقل الجديد الذي تم إضافته

        [Required]
        [Display(Name = "اسم الملف")]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "مسار الملف (URL/Path)")]
        public string FilePath { get; set; } = string.Empty;

        [Display(Name = "تاريخ الرفع")]
        public DateTime UploadDate { get; set; } = DateTime.Now;

        // مفاتيح أجنبية مرنة: المستند قد يخص سيارة، أو عميل، أو شحنة
        public int? CarId { get; set; }
        [ForeignKey("CarId")]
        public virtual Car? Car { get; set; }

        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public int? ShipmentId { get; set; }
        [ForeignKey("ShipmentId")]
        public virtual Shipment? Shipment { get; set; }
    }
}