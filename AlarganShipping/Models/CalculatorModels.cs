namespace AlarganShipping.Models
{
    public class CalculatorRequest
    {
        public decimal SalePrice { get; set; }
        public int CarType { get; set; } // 1 = Big, 2 = Mid, 3 = Small
        public decimal OceanFreight { get; set; } = 300.00m; // تكلفة الشحن البحري الافتراضية
    }

    public class CalculatorResponse
    {
        // Auction
        public decimal SalePrice { get; set; }
        public decimal BuyerFee { get; set; }
        public decimal EnvironmentalFee { get; set; }
        public decimal VirtualBidFee { get; set; }
        public decimal GateFee { get; set; }
        public decimal TitlePickupFee { get; set; }
        public decimal TotalInAuctionUSD { get; set; }
        public decimal TotalOMR { get; set; }

        // Shipping
        public decimal SeaShipping { get; set; }
        public decimal TaxCard { get; set; }
        public decimal PaymentService { get; set; }
        public decimal Tax { get; set; }
        public decimal Customs { get; set; }
        public decimal CustomsVAT { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal OfficeCommission { get; set; }
        public decimal SuvCarTypeFee { get; set; }
        public decimal TotalShipping { get; set; }

        // Totals
        public decimal FinalTotal { get; set; }
        public decimal TotalDueNow { get; set; }
        public decimal TotalDueOnArrival { get; set; }
    }
}