// دالة لحساب رسوم المشتري (Buyer Fee) بناءً على جدول العمولات المرفق
function getBuyerFee(salePrice) {
    if (salePrice <= 0) return 0;
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

    // بناءً على الصورة، مبلغ 1500 يعطي عمولة 315 (مقاربة لأسعار IAAI)
    if (salePrice >= 1500 && salePrice <= 1599.99) return 315.00;

    // للقيم الأعلى يتم حساب نسبة مئوية تقريبية (يمكن للمستخدم تعديلها يدوياً في الحاسبة)
    if (salePrice >= 1600) {
        return 315.00 + ((salePrice - 1500) * 0.02);
    }

    return 0;
}

// دالة لحساب رسوم المزاد الإجمالية
function calculateTotalAuctionFees(salePrice) {
    if (!salePrice || salePrice <= 0) {
        return { buyerFee: 0, totalFees: 0, fixedFees: 0 };
    }

    const buyerFee = getBuyerFee(salePrice);

    // الرسوم الثابتة (بيئة، بوابة، الخ)
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