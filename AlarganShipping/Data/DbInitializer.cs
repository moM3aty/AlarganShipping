// مسار الملف: Data/DbInitializer.cs
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

            if (context.Cars.Any())
            {
                return; // البيانات موجودة بالفعل
            }

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
            }

            // 1. إضافة الموانئ
            var locations = new List<Location>
            {
                new Location { Name = "New Jersey Port", LocationType = "Port", Country = "USA", StateOrProvince = "NJ", ContactPhone = "+1 000 000", Address = "-", ContactPerson = "-" },
                new Location { Name = "Texas Warehouse", LocationType = "Warehouse", Country = "USA", StateOrProvince = "TX", ContactPhone = "+1 000 000", Address = "-", ContactPerson = "-" },
                new Location { Name = "ميناء صلالة", LocationType = "Port", Country = "Oman", StateOrProvince = "Dhofar", ContactPhone = "00000000", Address = "-", ContactPerson = "-" },
                new Location { Name = "ميناء صحار", LocationType = "Port", Country = "Oman", StateOrProvince = "Al Batinah", ContactPhone = "00000000", Address = "-", ContactPerson = "-" }
            };
            context.Locations.AddRange(locations);
            context.SaveChanges();

            // 💡 التعديل هنا: إضافة BuyerNumber لحل مشكلة جدول Auctions
            var auctions = new List<Auction>
            {
                new Auction { Name = "Copart", Location = "Multiple", DefaultAuctionFee = 700, BuyerNumber = "BUYER-1001" },
                new Auction { Name = "IAAI", Location = "Multiple", DefaultAuctionFee = 650, BuyerNumber = "BUYER-1002" }
            };
            context.Auctions.AddRange(auctions);
            context.SaveChanges();

            var customers = new List<Customer>
            {
                new Customer { Name = "Ahmed Al-Said", Phone = "+968 7766 5544", Email = "ahmed@example.com", CivilId = "123456789", Address = "مسقط، المعبيلة", TotalPaid = 12000, TotalBalance = 5050, CustomerCode = "CUST-001", PortalUsername = "ahmed_said", PortalPassword = "password123", IsPortalActive = true },
                new Customer { Name = "Mazin Al-Balushi", Phone = "+968 9988 7766", Email = "mazin@example.com", CivilId = "987654321", Address = "صحار", TotalPaid = 23900, TotalBalance = 0, CustomerCode = "CUST-002", PortalUsername = "mazin_b", PortalPassword = "password123", IsPortalActive = true },
                new Customer { Name = "Khalid Al-Oufi", Phone = "+968 9123 4567", Email = "khalid@example.com", CivilId = "456123789", Address = "نزوى", TotalPaid = 0, TotalBalance = 15000, CustomerCode = "CUST-003", PortalUsername = "khalid_o", PortalPassword = "password123", IsPortalActive = true }
            };
            context.Customers.AddRange(customers);
            context.SaveChanges();

            var shipments = new List<Shipment>
            {
                new Shipment { ContainerNumber = "MSKU1234567", BookingNumber = "BKG-998877", ShippingLine = "Maersk", LoadingPortId = locations[0].Id, DischargePortId = locations[3].Id, ExpectedTimeOfDeparture = DateTime.Now.AddDays(-15), ExpectedTimeOfArrival = DateTime.Now.AddDays(10), Status = "Sailed" },
                new Shipment { ContainerNumber = "TGHU9876543", BookingNumber = "BKG-554433", ShippingLine = "MSC", LoadingPortId = locations[0].Id, DischargePortId = locations[2].Id, ExpectedTimeOfDeparture = DateTime.Now.AddDays(5), ExpectedTimeOfArrival = DateTime.Now.AddDays(35), Status = "Pending" }
            };
            context.Shipments.AddRange(shipments);
            context.SaveChanges();

            var cars = new List<Car>
            {
                new Car { VIN = "1N4AL3AP1N123456", Make = "Toyota", Model = "Camry SE", Year = 2022, Color = "Silver", PurchasePrice = 17150, InternalCode = "AUTO-2024-001", StatusId = 3, CustomerId = customers[0].Id, AuctionId = auctions[0].Id, ShipmentId = shipments[0].Id },
                new Car { VIN = "5UXWX7C5XB987654", Make = "BMW", Model = "X5 xDrive40i", Year = 2021, Color = "White", PurchasePrice = 23900, InternalCode = "AUTO-2024-002", StatusId = 4, CustomerId = customers[1].Id, AuctionId = auctions[1].Id, ShipmentId = shipments[0].Id },
                new Car { VIN = "2T2HZMAAXP112233", Make = "Lexus", Model = "RX 350", Year = 2023, Color = "Black", PurchasePrice = 15000, InternalCode = "AUTO-2024-003", StatusId = 1, CustomerId = customers[2].Id, AuctionId = auctions[0].Id, ShipmentId = null }
            };
            context.Cars.AddRange(cars);
            context.SaveChanges();

            var trackingLogs = new List<TrackingLog>
            {
                new TrackingLog { CarId = cars[0].Id, Title = "تم الشراء من المزاد", Location = "Copart NJ", Description = "تم شراء السيارة بنجاح ودفع الرسوم.", ProgressPercentage = 10, UpdateDate = DateTime.Now.AddDays(-20) },
                new TrackingLog { CarId = cars[0].Id, Title = "السيارة في المستودع", Location = "New Jersey Warehouse", Description = "وصلت السيارة إلى مستودع التجميع بانتظار التحميل.", ProgressPercentage = 33, UpdateDate = DateTime.Now.AddDays(-18) },
                new TrackingLog { CarId = cars[0].Id, Title = "أبحرت السفينة", Location = "New Jersey Port", Description = "تم تحميل السيارة في الحاوية والسفينة في طريقها.", ProgressPercentage = 66, UpdateDate = DateTime.Now.AddDays(-15) }
            };
            context.TrackingLogs.AddRange(trackingLogs);

            context.SaveChanges();
        }
    }
}