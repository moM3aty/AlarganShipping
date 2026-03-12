using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    public class DispatchOrder
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "رقم الأمر")]
        public string? OrderNumber { get; set; }

        public int CarId { get; set; }
        [ForeignKey("CarId")]
        public Car? Car { get; set; }

        public int TransporterId { get; set; }
        [ForeignKey("TransporterId")]
        public Transporter? Transporter { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TowingFee { get; set; }

        public DateTime DispatchDate { get; set; }
        public string? Status { get; set; } 

        public string? DriverNotes { get; set; }

        [Display(Name = "اسم السائق / المستلم")]
        public string? DriverName { get; set; }

        [Display(Name = "رقم هاتف السائق")]
        public string? DriverPhone { get; set; }

        [Display(Name = "شركة النقل (إن وجدت)")]
        public string? TowingCompany { get; set; }

        [Display(Name = "ملاحظات التسليم")]
        public string? Notes { get; set; }

        public int OriginLocationId { get; set; }
        [ForeignKey("OriginLocationId")]
        public Location? OriginLocation { get; set; }

        public int DestinationLocationId { get; set; }
        [ForeignKey("DestinationLocationId")]
        public Location? DestinationLocation { get; set; }
    }
}