    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace AlarganShipping.Models
    {
        // نموذج المواقع والموانئ مكتمل 100%
        public class Location
        {
            [Key]
            public int Id { get; set; }

            [Required(ErrorMessage = "اسم الموقع مطلوب")]
            [Display(Name = "اسم الموقع / الميناء")]
            public string Name { get; set; } // مثال: New Jersey Warehouse

            [Display(Name = "نوع الموقع")]
            public string LocationType { get; set; } // Warehouse, Port, Branch

            [Display(Name = "الدولة")]
            public string Country { get; set; }

            [Display(Name = "الولاية / المحافظة")]
            public string StateOrProvince { get; set; }

            [Display(Name = "العنوان التفصيلي")]
            public string Address { get; set; }

            [Display(Name = "الشخص المسؤول")]
            public string ContactPerson { get; set; }

            [Display(Name = "رقم التواصل")]
            public string ContactPhone { get; set; }

            // العلاقات: أوامر النقل الداخلي المرتبطة بهذا الموقع
            [InverseProperty("OriginLocation")]
            public virtual ICollection<DispatchOrder> OriginDispatchOrders { get; set; }

            [InverseProperty("DestinationLocation")]
            public virtual ICollection<DispatchOrder> DestinationDispatchOrders { get; set; }
        }
    }