using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        public string Name { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string CivilId { get; set; }
        public string Address { get; set; }

        public string CustomerCode { get; set; }
        public string PortalUsername { get; set; }
        public string PortalPassword { get; set; }
        public bool IsPortalActive { get; set; } = true;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBalance { get; set; }

        public ICollection<Car> Cars { get; set; } = new List<Car>();
        public ICollection<PaymentReceipt> PaymentReceipts { get; set; } = new List<PaymentReceipt>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}