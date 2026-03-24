// wwwroot/js/auctionCalculator.js

// دالة لحساب رسوم المشتري (Buyer Fee) بناءً على جدول العمولات الجديد (Sheet2)
function getBuyerFee(salePrice) {
    if (salePrice <= 0) return 0;

    // نطاقات الأسعار بناءً على الملف المحدث
    if (salePrice >= 0 && salePrice <= 49.99) return 25.00;
    if (salePrice >= 50 && salePrice <= 99.99) return 45.00;
    if (salePrice >= 100 && salePrice <= 199.99) return 80.00;
    if (salePrice >= 200 && salePrice <= 299.99) return 130.00;
    if (salePrice >= 300 && salePrice <= 349.99) return 132.50;
    if (salePrice >= 350 && salePrice <= 399.99) return 135.00;
    if (salePrice >= 400 && salePrice <= 449.99) return 170.00;
    if (salePrice >= 450 && salePrice <= 499.99) return 180.00;
    if (salePrice >= 500 && salePrice <= 549.99) return 200.00;
    if (salePrice >= 550 && salePrice <= 599.99) return 205.00;
    if (salePrice >= 600 && salePrice <= 699.99) return 235.00;
    if (salePrice >= 700 && salePrice <= 799.99) return 260.00;
    if (salePrice >= 800 && salePrice <= 899.99) return 280.00;
    if (salePrice >= 900 && salePrice <= 999.99) return 305.00;
    if (salePrice >= 1000 && salePrice <= 1199.99) return 355.00;
    if (salePrice >= 1200 && salePrice <= 1299.99) return 380.00;
    if (salePrice >= 1300 && salePrice <= 1399.99) return 400.00;
    if (salePrice >= 1400 && salePrice <= 1499.99) return 410.00;

    // التعديل الهام بناءً على الملف: 1500 - 1599.99 = 420.00
    if (salePrice >= 1500 && salePrice <= 1599.99) return 420.00;
    if (salePrice >= 1600 && salePrice <= 1699.99) return 440.00;
    if (salePrice >= 1700 && salePrice <= 1799.99) return 450.00;
    if (salePrice >= 1800 && salePrice <= 1999.99) return 465.00;
    if (salePrice >= 2000 && salePrice <= 2399.99) return 500.00;
    if (salePrice >= 2400 && salePrice <= 2499.99) return 525.00;
    if (salePrice >= 2500 && salePrice <= 2999.99) return 550.00;
    if (salePrice >= 3000 && salePrice <= 3499.99) return 650.00;
    if (salePrice >= 3500 && salePrice <= 3999.99) return 700.00;
    if (salePrice >= 4000 && salePrice <= 4499.99) return 725.00;
    if (salePrice >= 4500 && salePrice <= 4999.99) return 750.00;
    if (salePrice >= 5000 && salePrice <= 5499.99) return 775.00;
    if (salePrice >= 5500 && salePrice <= 5999.99) return 775.00;
    if (salePrice >= 6000 && salePrice <= 6499.99) return 800.00;
    if (salePrice >= 6500 && salePrice <= 6999.99) return 800.00;
    if (salePrice >= 7000 && salePrice <= 7499.99) return 825.00;
    if (salePrice >= 7500 && salePrice <= 7999.99) return 825.00;
    if (salePrice >= 8000 && salePrice <= 8499.99) return 850.00;
    if (salePrice >= 8500 && salePrice <= 8999.99) return 850.00;
    if (salePrice >= 9000 && salePrice <= 9999.99) return 850.00;
    if (salePrice >= 10000 && salePrice <= 10499.99) return 900.00;
    if (salePrice >= 10500 && salePrice <= 10999.99) return 900.00;
    if (salePrice >= 11000 && salePrice <= 11499.99) return 900.00;
    if (salePrice >= 11500 && salePrice <= 11999.99) return 900.00;
    if (salePrice >= 12000 && salePrice <= 12499.99) return 900.00;
    if (salePrice >= 12500 && salePrice <= 14999.99) return 900.00;

    // للقيم الأعلى يتم حساب نسبة 7.5% كما هو موضح في نهاية الجدول
    if (salePrice >= 15000) {
        return salePrice * 0.075; // 7.50%
    }

    return 0;
}

// دالة لحساب رسوم المزاد الإجمالية
function calculateTotalAuctionFees(salePrice) {
    if (!salePrice || salePrice <= 0) {
        return { buyerFee: 0, totalFees: 0, fixedFees: 0 };
    }

    const buyerFee = getBuyerFee(salePrice);

    // الرسوم الثابتة (بيئة، بوابة، الخ) - مأخوذة من ورقة1
    const environmentalFee = 15;
    const virtualBidFee = 95;
    const gateFee = 95;
    const titlePickupFee = 40;

    const totalFixedFees = environmentalFee + virtualBidFee + gateFee + titlePickupFee;
    const totalFees = buyerFee + totalFixedFees;

    return {
        buyerFee: buyerFee,
        fixedFees: totalFixedFees,
        totalFees: totalFees
    };
}