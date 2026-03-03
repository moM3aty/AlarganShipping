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
            context.Database.EnsureCreated();

            // 1. إضافة المستخدم (Admin) وحفظه فوراً في قاعدة البيانات أولاً
            if (!context.Users.Any(u => u.Email == "admin@alargan.com"))
            {
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
                context.SaveChanges(); // حفظ فوري للمستخدم لضمان القدرة على تسجيل الدخول
            }

            // التحقق من وجود بيانات للسيارات لتجنب التكرار
            if (context.Cars.Any())
            {
                return;
            }

            // 2. إضافة الموانئ والمستودعات (مع إعطاء قيم لكل الحقول لتجنب الـ NULL)
            var locations = new List<Location>
            {
                new Location { Name = "New Jersey Port", LocationType = "Port", Country = "USA", StateOrProvince = "NJ", Address = "Port Area", ContactPerson = "Port Admin", ContactPhone = "+1000000000" },
                new Location { Name = "Texas Warehouse", LocationType = "Warehouse", Country = "USA", StateOrProvince = "TX", Address = "Warehouse Area", ContactPerson = "WH Admin", ContactPhone = "+1000000000" },
                new Location { Name = "ميناء صلالة", LocationType = "Port", Country = "Oman", StateOrProvince = "Dhofar", Address = "صلالة", ContactPerson = "إدارة الميناء", ContactPhone = "90000000" },
                new Location { Name = "ميناء صحار", LocationType = "Port", Country = "Oman", StateOrProvince = "Al Batinah", Address = "صحار", ContactPerson = "إدارة الميناء", ContactPhone = "90000000" }
            };
            context.Locations.AddRange(locations);
            context.SaveChanges();

            // 3. إضافة المزادات
            var auctions = new List<Auction>
            {
                new Auction { Name = "Copart", Location = "Multiple", DefaultAuctionFee = 700, BuyerNumber = "BUYER-1001" },
                new Auction { Name = "IAAI", Location = "Multiple", DefaultAuctionFee = 650, BuyerNumber = "BUYER-1002" }
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
                new Shipment { ContainerNumber = "MSKU1234567", BookingNumber = "BKG-998877", BillOfLading = "BL-001", ShippingLine = "Maersk", LoadingPortId = locations[0].Id, DischargePortId = locations[3].Id, ExpectedTimeOfDeparture = DateTime.Now.AddDays(-15), ExpectedTimeOfArrival = DateTime.Now.AddDays(10), Status = "Sailed" },
                new Shipment { ContainerNumber = "TGHU9876543", BookingNumber = "BKG-554433", BillOfLading = "BL-002", ShippingLine = "MSC", LoadingPortId = locations[0].Id, DischargePortId = locations[2].Id, ExpectedTimeOfDeparture = DateTime.Now.AddDays(5), ExpectedTimeOfArrival = DateTime.Now.AddDays(35), Status = "Pending" }
            };
            context.Shipments.AddRange(shipments);
            context.SaveChanges();

            // 6. إضافة السيارات
            var cars = new List<Car>
            {
                new Car {
                    VIN = "1N4AL3AP1N123456", Make = "Toyota", Model = "Camry SE", Year = 2022, Color = "Silver",
                    PurchasePrice = 17150, InternalCode = "AUTO-2024-001", StatusId = 3, CustomerId = customers[0].Id,
                    AuctionId = auctions[0].Id, ShipmentId = shipments[0].Id,
                    PlateNo = "-", CurrentLocation = "Copart NJ", CardNo = "-", EngineNo = "-",
                    TaxType = "-", CustomsDeclaration = "-", InsuranceNo = "-", VehicleType = "Sedan",
                    TaxMethod = "-", VehicleShape = "-", LotNumber = "-"
                },
                new Car {
                    VIN = "5UXWX7C5XB987654", Make = "BMW", Model = "X5 xDrive40i", Year = 2021, Color = "White",
                    PurchasePrice = 23900, InternalCode = "AUTO-2024-002", StatusId = 4, CustomerId = customers[1].Id,
                    AuctionId = auctions[1].Id, ShipmentId = shipments[0].Id,
                    PlateNo = "-", CurrentLocation = "Salalah Port", CardNo = "-", EngineNo = "-",
                    TaxType = "-", CustomsDeclaration = "-", InsuranceNo = "-", VehicleType = "SUV",
                    TaxMethod = "-", VehicleShape = "-", LotNumber = "-"
                }
            };
            context.Cars.AddRange(cars);
            context.SaveChanges();

            // 7. إضافة سجلات التتبع
            var trackingLogs = new List<TrackingLog>
            {
                new TrackingLog { CarId = cars[0].Id, Title = "تم الشراء من المزاد", Location = "Copart NJ", Description = "تم شراء السيارة بنجاح ودفع الرسوم.", ProgressPercentage = 10, UpdateDate = DateTime.Now.AddDays(-20) },
                new TrackingLog { CarId = cars[0].Id, Title = "السيارة في المستودع", Location = "New Jersey Warehouse", Description = "وصلت السيارة إلى مستودع التجميع بانتظار التحميل.", ProgressPercentage = 33, UpdateDate = DateTime.Now.AddDays(-18) }
            };
            context.TrackingLogs.AddRange(trackingLogs);
            context.SaveChanges();
        }
    }
}