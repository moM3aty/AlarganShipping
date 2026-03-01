using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    // نموذج أوامر النقل الداخلي (التكليف بالنقل من المزاد للمستودع) مكتمل 100%
    public class DispatchOrder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "رقم أمر النقل")]
        public string OrderNumber { get; set; }

        [Display(Name = "تكلفة النقل المتفق عليها ($)")]
        public decimal TowingFee { get; set; }

        [Display(Name = "تاريخ التكليف")]
        public DateTime DispatchDate { get; set; } = DateTime.Now;

        [Display(Name = "تاريخ الاستلام الفعلي (Pickup)")]
        public DateTime? ActualPickupDate { get; set; }

        [Display(Name = "تاريخ التسليم الفعلي (Delivery)")]
        public DateTime? ActualDeliveryDate { get; set; }

        [Display(Name = "حالة أمر النقل")]
        public string Status { get; set; } = "Assigned"; // Assigned, PickedUp, Delivered, Cancelled

        [Display(Name = "ملاحظات السائق (أعطال/نواقص)")]
        public string DriverNotes { get; set; }

        // العلاقات: أمر النقل يخص سيارة معينة
        [Required]
        public int CarId { get; set; }
        [ForeignKey("CarId")]
        public virtual Car Car { get; set; }

        // العلاقات: الناقل المُكلف (شركة الريكفري)
        [Required]
        public int TransporterId { get; set; }
        [ForeignKey("TransporterId")]
        public virtual Transporter Transporter { get; set; }

        // مسار النقل: من (مثلاً المزاد) إلى (مثلاً المستودع)
        public int? OriginLocationId { get; set; }
        [ForeignKey("OriginLocationId")]
        public virtual Location OriginLocation { get; set; }

        public int? DestinationLocationId { get; set; }
        [ForeignKey("DestinationLocationId")]
        public virtual Location DestinationLocation { get; set; }
    }
}