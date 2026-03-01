using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AlarganShipping.Models
{
    // نموذج المزادات مكتمل 100%
    public class Auction
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المزاد مطلوب")]
        [Display(Name = "اسم المزاد")]
        public string Name { get; set; } // مثال: Copart, IAAI

        [Display(Name = "موقع المزاد (الولاية/المدينة)")]
        public string Location { get; set; }

        [Display(Name = "رقم المشتري (Buyer Number)")]
        public string BuyerNumber { get; set; }

        [Display(Name = "رسوم المزاد الافتراضية ($)")]
        public decimal DefaultAuctionFee { get; set; }

        // العلاقات: السيارات المشتراة من هذا المزاد
        public virtual ICollection<Car> Cars { get; set; }
    }
}