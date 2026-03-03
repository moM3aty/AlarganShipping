using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    public class Shipment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "رقم الحاوية مطلوب")]
        [Display(Name = "رقم الحاوية (Container No)")]
        public string ContainerNumber { get; set; }

        [Display(Name = "رقم الحجز (Booking No)")]
        public string? BookingNumber { get; set; }

        [Display(Name = "رقم بوليصة الشحن (B/L)")]
        public string? BillOfLading { get; set; }

        [Display(Name = "خط الملاحة")]
        public string? ShippingLine { get; set; }

        [Display(Name = "تاريخ الإبحار المتوقع (ETD)")]
        public DateTime? ExpectedTimeOfDeparture { get; set; }

        [Display(Name = "تاريخ الوصول المتوقع (ETA)")]
        public DateTime? ExpectedTimeOfArrival { get; set; }

        [Display(Name = "حالة الشحنة")]
        public string? Status { get; set; } = "Pending";

        public int? LoadingPortId { get; set; }
        [ForeignKey("LoadingPortId")]
        public virtual Location? LoadingPort { get; set; }

        public int? DischargePortId { get; set; }
        [ForeignKey("DischargePortId")]
        public virtual Location? DischargePort { get; set; }

        public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
        public virtual ICollection<DocumentAttachment> Documents { get; set; } = new List<DocumentAttachment>();
    }
}