using System;
using System.Collections.Generic;
using System.Linq;
using AlarganShipping.Models;

namespace AlarganShipping.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // التأكد من إنشاء قاعدة البيانات
            context.Database.EnsureCreated();

            // التحقق مما إذا كانت قاعدة البيانات تحتوي على عملاء مسبقاً (لمنع التكرار)
            if (context.Customers.Any())
            {
                return; // قاعدة البيانات مهيأة مسبقاً
            }

            // 1. إضافة المستخدم الافتراضي (المدير)
            var adminUser = new User
            {
                FullName = "مدير النظام",
                Email = "admin@alargan.com",
                PasswordHash = "123456",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            context.Users.Add(adminUser);

            // 2. إضافة الموانئ والمواقع
            var locations = new List<Location>
            {
                new Location { Name = "New Jersey Port", LocationType = "Port", Country = "USA", StateOrProvince = "NJ" },
                new Location { Name = "Texas Warehouse", LocationType = "Warehouse", Country = "USA", StateOrProvince = "TX" },
                new Location { Name = "ميناء صلالة", LocationType = "Port", Country = "Oman", StateOrProvince = "Dhofar" },
                new Location { Name = "ميناء صحار", LocationType = "Port", Country = "Oman", StateOrProvince = "Al Batinah" }
            };
            context.Locations.AddRange(locations);
            context.SaveChanges();

            // 3. إضافة المزادات
            var auctions = new List<Auction>
            {
                new Auction { Name = "Copart", Location = "Multiple", DefaultAuctionFee = 700 },
                new Auction { Name = "IAAI", Location = "Multiple", DefaultAuctionFee = 650 }
            };
            context.Auctions.AddRange(auctions);
            context.SaveChanges();

            // 4. إضافة العملاء
            var customers = new List<Customer>
            {
                new Customer { Name = "Ahmed Al-Said", Phone = "+968 7766 5544", Email = "ahmed@example.com", CivilId = "123456789", Address = "مسقط، المعبيلة", TotalPaid = 12000, TotalBalance = 5050, CustomerCode = "CUST-001", PortalUsername = "ahmed_said", PortalPassword = "password123", IsPortalActive = true },
                new Customer { Name = "Mazin Al-Balushi", Phone = "+968 9988 7766", Email = "mazin@example.com", CivilId = "987654321", Address = "صحار", TotalPaid = 23900, TotalBalance = 0, CustomerCode = "CUST-002", PortalUsername = "mazin_b", PortalPassword = "password123", IsPortalActive = true },
                new Customer { Name = "Khalid Al-Oufi", Phone = "+968 9123 4567", Email = "khalid@example.com", CivilId = "456123789", Address = "نزوى", TotalPaid = 0, TotalBalance = 15000, CustomerCode = "CUST-003", PortalUsername = "khalid_o", PortalPassword = "password123", IsPortalActive = true }
            };
            context.Customers.AddRange(customers);
            context.SaveChanges();

            // 5. إضافة الشحنات
            var shipments = new List<Shipment>
            {
                new Shipment { ContainerNumber = "MSKU1234567", BookingNumber = "BKG-998877", ShippingLine = "Maersk", LoadingPortId = locations[0].Id, DischargePortId = locations[3].Id, ExpectedTimeOfDeparture = DateTime.Now.AddDays(-15), ExpectedTimeOfArrival = DateTime.Now.AddDays(10), Status = "Sailed" },
                new Shipment { ContainerNumber = "TGHU9876543", BookingNumber = "BKG-554433", ShippingLine = "MSC", LoadingPortId = locations[0].Id, DischargePortId = locations[2].Id, ExpectedTimeOfDeparture = DateTime.Now.AddDays(5), ExpectedTimeOfArrival = DateTime.Now.AddDays(35), Status = "Pending" }
            };
            context.Shipments.AddRange(shipments);
            context.SaveChanges();

            // 6. إضافة السيارات
            var cars = new List<Car>
            {
                new Car { VIN = "1N4AL3AP1N123456", Make = "Toyota", Model = "Camry SE", Year = 2022, Color = "Silver", PurchasePrice = 17150, InternalCode = "AUTO-2024-001", StatusId = 3, CustomerId = customers[0].Id, AuctionId = auctions[0].Id, ShipmentId = shipments[0].Id },
                new Car { VIN = "5UXWX7C5XB987654", Make = "BMW", Model = "X5 xDrive40i", Year = 2021, Color = "White", PurchasePrice = 23900, InternalCode = "AUTO-2024-002", StatusId = 4, CustomerId = customers[1].Id, AuctionId = auctions[1].Id, ShipmentId = shipments[0].Id },
                new Car { VIN = "2T2HZMAAXP112233", Make = "Lexus", Model = "RX 350", Year = 2023, Color = "Black", PurchasePrice = 15000, InternalCode = "AUTO-2024-003", StatusId = 1, CustomerId = customers[2].Id, AuctionId = auctions[0].Id, ShipmentId = null }
            };
            context.Cars.AddRange(cars);
            context.SaveChanges();

            // 7. إضافة سجلات التتبع (Tracking Logs)
            var trackingLogs = new List<TrackingLog>
            {
                new TrackingLog { CarId = cars[0].Id, Title = "تم الشراء من المزاد", Location = "Copart NJ", Description = "تم شراء السيارة بنجاح ودفع الرسوم.", ProgressPercentage = 10, UpdateDate = DateTime.Now.AddDays(-20) },
                new TrackingLog { CarId = cars[0].Id, Title = "السيارة في المستودع", Location = "New Jersey Warehouse", Description = "وصلت السيارة إلى مستودع التجميع بانتظار التحميل.", ProgressPercentage = 33, UpdateDate = DateTime.Now.AddDays(-18) },
                new TrackingLog { CarId = cars[0].Id, Title = "أبحرت السفينة", Location = "New Jersey Port", Description = "تم تحميل السيارة في الحاوية والسفينة في طريقها إلى عمان.", ProgressPercentage = 66, UpdateDate = DateTime.Now.AddDays(-15) },

                new TrackingLog { CarId = cars[1].Id, Title = "تم الشراء من المزاد", Location = "IAAI TX", Description = "عملية الشراء مكتملة.", ProgressPercentage = 10, UpdateDate = DateTime.Now.AddDays(-30) },
                new TrackingLog { CarId = cars[1].Id, Title = "وصلت إلى الميناء الوجهة", Location = "ميناء صحار", Description = "السيارة جاهزة للتخليص الجمركي.", ProgressPercentage = 100, UpdateDate = DateTime.Now.AddDays(-2) }
            };
            context.TrackingLogs.AddRange(trackingLogs);

            // 8. إضافة الفواتير
            var invoices = new List<Invoice>
            {
                new Invoice { InvoiceNumber = "INV-" + DateTime.Now.ToString("yyyyMMdd") + "01", IssueDate = DateTime.Now.AddDays(-20), CarId = cars[0].Id, CustomerId = customers[0].Id, CarPrice = 14000, AuctionFees = 700, LandFreight = 250, SeaFreight = 1200, CustomsFees = 600, AdminFees = 400, TotalAmount = 17150, IsPaid = false },
                new Invoice { InvoiceNumber = "INV-" + DateTime.Now.ToString("yyyyMMdd") + "02", IssueDate = DateTime.Now.AddDays(-30), CarId = cars[1].Id, CustomerId = customers[1].Id, CarPrice = 20000, AuctionFees = 650, LandFreight = 300, SeaFreight = 1400, CustomsFees = 850, AdminFees = 700, TotalAmount = 23900, IsPaid = true }
            };
            context.Invoices.AddRange(invoices);

            // 9. إضافة سندات القبض
            var receipts = new List<PaymentReceipt>
            {
                new PaymentReceipt { ReceiptNumber = "REC-" + DateTime.Now.ToString("yyyyMMdd") + "01", CustomerId = customers[0].Id, Amount = 12000, PaymentDate = DateTime.Now.AddDays(-10), PaymentMethod = "Bank Transfer", ReferenceNumber = "TRX-88776655", Notes = "دفعة أولى للسيارة الكامري" },
                new PaymentReceipt { ReceiptNumber = "REC-" + DateTime.Now.ToString("yyyyMMdd") + "02", CustomerId = customers[1].Id, Amount = 23900, PaymentDate = DateTime.Now.AddDays(-5), PaymentMethod = "Cheque", ReferenceNumber = "CHQ-11223344", Notes = "سداد كامل للسيارة BMW" }
            };
            context.PaymentReceipts.AddRange(receipts);

            // 10. إضافة الإشعارات (Notifications)
            var notifications = new List<Notification>
            {
                new Notification { Title = "دفعة جديدة مستلمة", Message = "تم استلام دفعة بقيمة $12,000 من العميل Ahmed Al-Said.", Type = "InvoiceAlert", CreatedAt = DateTime.Now.AddMinutes(-15), IsRead = false, CustomerId = customers[0].Id },
                new Notification { Title = "تحديث حالة شحنة", Message = "الحاوية رقم MSKU1234567 أبحرت بنجاح وهي في طريقها إلى ميناء صحار.", Type = "TrackingUpdate", CreatedAt = DateTime.Now.AddHours(-2), IsRead = false },
                new Notification { Title = "سيارة جديدة مضافة", Message = "تم إضافة سيارة Lexus RX 350 للعميل Khalid Al-Oufi.", Type = "SystemAlert", CreatedAt = DateTime.Now.AddDays(-1), IsRead = true, CustomerId = customers[2].Id },
                new Notification { Title = "تنبيه نظام", Message = "تم أرشفة قاعدة البيانات بنجاح لآخر 30 يوماً.", Type = "SystemAlert", CreatedAt = DateTime.Now.AddDays(-3), IsRead = true }
            };
            context.Notifications.AddRange(notifications);

            context.SaveChanges();
        }
    }
}