using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    // نموذج المصروفات التشغيلية (لحساب صافي أرباح الشركة بدقة)
    public class Expense
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "بيان المصروف")]
        public string Title { get; set; } // مثال: "رسوم تحميل حاوية", "وقود ناقلة", "رواتب"

        [Required]
        [Display(Name = "المبلغ")]
        public decimal Amount { get; set; }

        [Display(Name = "تاريخ الصرف")]
        public DateTime ExpenseDate { get; set; } = DateTime.Now;

        [Display(Name = "تصنيف المصروف")]
        public string Category { get; set; } // PortFees, Fuel, Maintenance, Salaries, Office

        [Display(Name = "رقم الفاتورة / الإيصال المرجعي")]
        public string ReferenceNumber { get; set; }

        // ربط المصروف بمركز تكلفة (اختياري)
        // إذا كان المصروف يخص شحنة معينة لخصمها من أرباح هذه الشحنة
        public int? ShipmentId { get; set; }
        [ForeignKey("ShipmentId")]
        public Shipment Shipment { get; set; }

        // إذا كان المصروف يخص سيارة معينة
        public int? CarId { get; set; }
        [ForeignKey("CarId")]
        public Car Car { get; set; }
    }
}