using AlarganShipping.Models;
using System;

namespace AlarganShipping.Services
{
    public interface ICalculatorService
    {
        CalculatorResponse Calculate(CalculatorRequest request);
    }

    public class CalculatorService : ICalculatorService
    {
        private const decimal ExchangeRate = 0.386m;

        public CalculatorResponse Calculate(CalculatorRequest request)
        {
            var res = new CalculatorResponse();
            decimal p = request.SalePrice;

            // 1. Auction Section (USD)
            res.SalePrice = p;
            res.BuyerFee = GetBuyerFee(p);
            res.EnvironmentalFee = 15.00m;
            res.VirtualBidFee = GetVirtualBidFee(p);
            res.GateFee = 95.00m;
            res.TitlePickupFee = 40.00m;

            res.TotalInAuctionUSD = res.SalePrice + res.BuyerFee + res.EnvironmentalFee + res.VirtualBidFee + res.GateFee + res.TitlePickupFee;

            // TOTAL in OMR
            res.TotalOMR = Math.Round(res.TotalInAuctionUSD * ExchangeRate, 2);

            // 2. Shipping Section (OMR)
            res.SeaShipping = res.TotalOMR + request.OceanFreight; // =B9+300
            res.TaxCard = 0.00m;
            res.PaymentService = 15.48m;
            res.Tax = Math.Round(res.SeaShipping * 0.05m, 2); // =B10*5/100
            res.Customs = 0.00m;
            res.CustomsVAT = Math.Round(res.Customs * 0.05m, 2); // =B15*5/100
            res.ShippingCost = 0.00m;
            res.OfficeCommission = 50.00m;
            res.SuvCarTypeFee = request.CarType == 1 ? 120.00m : (request.CarType == 2 ? 60.00m : 0.00m);

            res.TotalShipping = res.TaxCard + res.PaymentService + res.Tax + res.Customs + res.CustomsVAT +
                                res.ShippingCost + res.OfficeCommission + res.SuvCarTypeFee;

            // 3. Final Totals (OMR)
            res.FinalTotal = res.TotalShipping + res.TotalOMR; // =B21+B9
            res.TotalDueNow = res.TotalOMR + res.PaymentService + res.OfficeCommission; // =B9+B13+B18
            res.TotalDueOnArrival = res.FinalTotal - res.TotalDueNow; // =B24-C16

            return res;
        }

        private decimal GetBuyerFee(decimal p)
        {
            if (p >= 0 && p <= 99.99m) return 1;
            if (p >= 100 && p <= 199.99m) return 25;
            if (p >= 200 && p <= 299.99m) return 60;
            if (p >= 300 && p <= 349.99m) return 85;
            if (p >= 350 && p <= 399.99m) return 100;
            if (p >= 400 && p <= 449.99m) return 125;
            if (p >= 450 && p <= 499.99m) return 135;
            if (p >= 500 && p <= 549.99m) return 145;
            if (p >= 550 && p <= 599.99m) return 155;
            if (p >= 600 && p <= 699.99m) return 170;
            if (p >= 700 && p <= 799.99m) return 195;
            if (p >= 800 && p <= 899.99m) return 215;
            if (p >= 900 && p <= 999.99m) return 230;
            if (p >= 1000 && p <= 1199.99m) return 250;
            if (p >= 1200 && p <= 1299.99m) return 270;
            if (p >= 1300 && p <= 1399.99m) return 285;
            if (p >= 1400 && p <= 1499.99m) return 300;
            if (p >= 1500 && p <= 1599.99m) return 315;
            if (p >= 1600 && p <= 1699.99m) return 330;
            if (p >= 1700 && p <= 1799.99m) return 350;
            if (p >= 1800 && p <= 1999.99m) return 370;
            if (p >= 2000 && p <= 2399.99m) return 390;
            if (p >= 2400 && p <= 2499.99m) return 425;
            if (p >= 2500 && p <= 2999.99m) return 460;
            if (p >= 3000 && p <= 3499.99m) return 505;
            if (p >= 3500 && p <= 3999.99m) return 555;
            if (p >= 4000 && p <= 4499.99m) return 600;
            if (p >= 4500 && p <= 4999.99m) return 625;
            if (p >= 5000 && p <= 5499.99m) return 650;
            if (p >= 5500 && p <= 5999.99m) return 675;
            if (p >= 6000 && p <= 6499.99m) return 700;
            if (p >= 6500 && p <= 6999.99m) return 720;
            if (p >= 7000 && p <= 7499.99m) return 755;
            if (p >= 7500 && p <= 7999.99m) return 775;
            if (p >= 8000 && p <= 8499.99m) return 800;
            if (p >= 8500 && p <= 8999.99m) return 820;
            if (p >= 9000 && p <= 9999.99m) return 820;
            if (p >= 10000 && p <= 11499.99m) return 850;
            if (p >= 11500 && p <= 11999.99m) return 860;
            if (p >= 12000 && p <= 12499.99m) return 875;
            if (p >= 12500 && p <= 14999.99m) return 890;
            if (p >= 15000) return Math.Round(p * 0.06m, 2);
            return 0;
        }

        private decimal GetVirtualBidFee(decimal p)
        {
            if (p >= 0 && p <= 99.99m) return 0;
            if (p >= 100 && p <= 499.99m) return 50;
            if (p >= 500 && p <= 999.99m) return 65;
            if (p >= 1000 && p <= 1499.99m) return 85;
            if (p >= 1500 && p <= 1999.99m) return 95;
            if (p >= 2000 && p <= 3999.99m) return 110;
            if (p >= 4000 && p <= 5999.99m) return 125;
            if (p >= 6000 && p <= 7999.99m) return 145;
            if (p >= 8000) return 160;
            return 0;
        }
    }
}