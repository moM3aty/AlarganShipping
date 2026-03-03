using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarganShipping.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        public string InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }

        public int CarId { get; set; }
        [ForeignKey("CarId")]
        public Car Car { get; set; }

        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CarPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AuctionFees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LandFreight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SeaFreight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CustomsFees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AdminFees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public bool IsPaid { get; set; }
    }
}