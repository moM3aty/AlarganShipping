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

           
            if (!context.VehicleShapes.Any())
            {
                context.VehicleShapes.AddRange(
                    new VehicleShape { Name = "صالون (Sedan)" },
                    new VehicleShape { Name = "دفع رباعي (SUV)" },
                    new VehicleShape { Name = "بيك أب (Pickup)" },
                    new VehicleShape { Name = "كوبيه (Coupe)" },
                    new VehicleShape { Name = "فان (Van)" },
                    new VehicleShape { Name = "شاحنة خفيفة (Light Truck)" },
                    new VehicleShape { Name = "دراجة نارية (Motorcycle)" }
                );
                context.TaxTypes.AddRange(
                    new TaxType { Name = "ضريبة قيمة مضافة (VAT)" },
                    new TaxType { Name = "ضريبة جمركية (Customs)" },
                    new TaxType { Name = "رسوم أرضية ميناء" },
                    new TaxType { Name = "رسوم تخليص" }
                );
                context.TaxMethods.AddRange(
                    new TaxMethod { Name = "نسبة مئوية (%)" },
                    new TaxMethod { Name = "مبلغ مقطوع ($)" }
                );
                context.SaveChanges();
            }

 
            if (!context.Users.Any())
            {
                var users = new List<User>
                {
                    // الإدارة والمالية
                    new User { FullName = "مدير النظام", Email = "admin@alargan.com", PasswordHash = "123456", Role = "Admin", IsActive = true, CreatedAt = DateTime.Now.AddMonths(-12) },
                    new User { FullName = "محمود المالي", Email = "finance@alargan.com", PasswordHash = "123456", Role = "Accountant", IsActive = true, CanManageFinance = true, CanManageCustomers = true, ShowOnContactPage = true, JobTitle = "مدير الحسابات", PhoneNumber = "96890000001", DepartmentIcon = "fa-calculator", CreatedAt = DateTime.Now.AddMonths(-10) },
                    
                    // العمليات والشحن
                    new User { FullName = "سالم اللوجستي", Email = "ops@alargan.com", PasswordHash = "123456", Role = "Staff", IsActive = true, CanManageCars = true, ShowOnContactPage = true, JobTitle = "مسؤول العمليات والشحن", PhoneNumber = "96890000002", DepartmentIcon = "fa-ship", CreatedAt = DateTime.Now.AddMonths(-8) },
                    
                    // المبيعات والمزادات
                    new User { FullName = "أحمد المزادات", Email = "sales@alargan.com", PasswordHash = "123456", Role = "Staff", IsActive = true, CanManageCustomers = true, ShowOnContactPage = true, JobTitle = "قسم المزادات (أمريكا)", PhoneNumber = "96890000003", DepartmentIcon = "fa-gavel", CreatedAt = DateTime.Now.AddMonths(-6) },
                    new User { FullName = "علي المبيعات", Email = "sales2@alargan.com", PasswordHash = "123456", Role = "Staff", IsActive = true, CanManageCustomers = true, ShowOnContactPage = true, JobTitle = "المبيعات (السلطنة)", PhoneNumber = "96890000004", DepartmentIcon = "fa-tags", CreatedAt = DateTime.Now.AddMonths(-4) },
                    
                    // خدمة العملاء وفروع أخرى
                    new User { FullName = "فاطمة خدمة العملاء", Email = "cs@alargan.com", PasswordHash = "123456", Role = "Staff", IsActive = true, CanManageCustomers = true, CanManageCars = true, ShowOnContactPage = true, JobTitle = "خدمة العملاء والدعم", PhoneNumber = "96890000005", DepartmentIcon = "fa-headset", CreatedAt = DateTime.Now.AddMonths(-2) },
                    new User { FullName = "محمد - فرع الإمارات", Email = "dubai@alargan.com", PasswordHash = "123456", Role = "Staff", IsActive = true, CanManageCars = true, ShowOnContactPage = true, JobTitle = "مدير فرع الإمارات", PhoneNumber = "97150000000", DepartmentIcon = "fa-briefcase", CreatedAt = DateTime.Now.AddMonths(-1) }
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            if (context.Cars.Any()) return; // منع التكرار

  
            var locations = new List<Location>
            {
                // أمريكا
                new Location { Name = "New Jersey Port (NJP)", LocationType = "Port", Country = "USA", StateOrProvince = "NJ", Address = "Port Elizabeth", ContactPerson = "John Doe", ContactPhone = "+12015550123" },
                new Location { Name = "Savannah Port (SAV)", LocationType = "Port", Country = "USA", StateOrProvince = "GA", Address = "Savannah Terminal", ContactPerson = "Mike Smith", ContactPhone = "+19125550199" },
                new Location { Name = "Los Angeles Port (LAX)", LocationType = "Port", Country = "USA", StateOrProvince = "CA", Address = "Long Beach", ContactPerson = "Robert", ContactPhone = "+13105550777" },
                new Location { Name = "NJ Warehouse", LocationType = "Warehouse", Country = "USA", StateOrProvince = "NJ", Address = "Linden Auto Yard", ContactPerson = "Alex", ContactPhone = "+12015550999" },
                new Location { Name = "TX Warehouse", LocationType = "Warehouse", Country = "USA", StateOrProvince = "TX", Address = "Houston Auto Yard", ContactPerson = "Sam", ContactPhone = "+17135550888" },
                new Location { Name = "CA Warehouse", LocationType = "Warehouse", Country = "USA", StateOrProvince = "CA", Address = "Compton Storage", ContactPerson = "Kevin", ContactPhone = "+13105550666" },
                
                // عمان
                new Location { Name = "ميناء صلالة", LocationType = "Port", Country = "Oman", StateOrProvince = "Dhofar", Address = "صلالة، عمان", ContactPerson = "إدارة الميناء", ContactPhone = "96823210000" },
                new Location { Name = "ميناء صحار", LocationType = "Port", Country = "Oman", StateOrProvince = "Al Batinah", Address = "صحار، عمان", ContactPerson = "إدارة الميناء", ContactPhone = "96826850000" },
                
                // الإمارات
                new Location { Name = "Jebel Ali Port", LocationType = "Port", Country = "UAE", StateOrProvince = "Dubai", Address = "JAFZA", ContactPerson = "Ahmed", ContactPhone = "+97148000000" },
                new Location { Name = "مستودع الشارقة", LocationType = "Warehouse", Country = "UAE", StateOrProvince = "Sharjah", Address = "المنطقة الحرة بالشارقة", ContactPerson = "محمود", ContactPhone = "+97150111222" }
            };
            context.Locations.AddRange(locations);
            context.SaveChanges();

   
            var auctions = new List<Auction>
            {
                new Auction { Name = "Copart USA", Location = "USA - Multiple", DefaultAuctionFee = 750, BuyerNumber = "COP-887766" },
                new Auction { Name = "IAAI USA", Location = "USA - Multiple", DefaultAuctionFee = 680, BuyerNumber = "IAA-554433" },
                new Auction { Name = "Manheim", Location = "USA - Multiple", DefaultAuctionFee = 500, BuyerNumber = "MAN-112233" },
                new Auction { Name = "Adesa", Location = "USA - Multiple", DefaultAuctionFee = 450, BuyerNumber = "ADE-998877" },
                new Auction { Name = "Emirates Auction", Location = "UAE - Dubai", DefaultAuctionFee = 300, BuyerNumber = "EMA-102030" }
            };
            context.Auctions.AddRange(auctions);
            context.SaveChanges();

    
            var transporters = new List<Transporter>
            {
                new Transporter { Name = "Speedy US Towing", Phone = "+18005550001", Email = "dispatch@speedy.com", DotNumber = "DOT-123456", InsuranceDetails = "Progressive Auto #998", InsuranceExpiryDate = DateTime.Now.AddMonths(8), Rating = 5 },
                new Transporter { Name = "SafeMove Recovery", Phone = "+18005550002", Email = "info@safemove.com", DotNumber = "DOT-654321", InsuranceDetails = "Geico #776", InsuranceExpiryDate = DateTime.Now.AddMonths(2), Rating = 4 },
                new Transporter { Name = "Texas Eagle Transport", Phone = "+18005550003", Email = "eagle@txtransport.com", DotNumber = "DOT-112233", InsuranceDetails = "StateFarm #445", InsuranceExpiryDate = DateTime.Now.AddDays(-10), Rating = 3 },
                new Transporter { Name = "West Coast Logistics", Phone = "+18005550004", Email = "west@wcl.com", DotNumber = "DOT-998877", InsuranceDetails = "Allstate #332", InsuranceExpiryDate = DateTime.Now.AddMonths(12), Rating = 5 },
                new Transporter { Name = "صقور الصحراء للريكفري", Phone = "+97150999888", Email = "desert@recovery.ae", DotNumber = "UAE-001", InsuranceDetails = "Oman Insurance #112", InsuranceExpiryDate = DateTime.Now.AddMonths(5), Rating = 4 }
            };
            context.Transporters.AddRange(transporters);
            context.SaveChanges();

 
            var customers = new List<Customer>
            {
                new Customer { Name = "شركة القمة للسيارات", Phone = "+968 7766 5544", Email = "info@alqimma.om", CivilId = "CR-102938", Address = "مسقط، المعبيلة الصناعية", TotalPaid = 0, TotalBalance = 0, CustomerCode = "CUST-001", PortalUsername = "alqimma", PortalPassword = "password123", IsPortalActive = true },
                new Customer { Name = "سعيد بن خلفان البلوشي", Phone = "+968 9988 7766", Email = "saeed@gmail.com", CivilId = "123456789", Address = "صحار، فلج القبائل", TotalPaid = 0, TotalBalance = 0, CustomerCode = "CUST-002", PortalUsername = "saeed", PortalPassword = "password123", IsPortalActive = true },
                new Customer { Name = "مازن الرواحي", Phone = "+968 9123 4567", Email = "mazin@hotmail.com", CivilId = "987654321", Address = "نزوى", TotalPaid = 0, TotalBalance = 0, CustomerCode = "CUST-003", PortalUsername = "mazin", PortalPassword = "password123", IsPortalActive = true },
                new Customer { Name = "مؤسسة خط الأفق", Phone = "+968 9234 5678", Email = "horizon@horizon.om", CivilId = "CR-556677", Address = "صلالة، عوقد", TotalPaid = 0, TotalBalance = 0, CustomerCode = "CUST-004", PortalUsername = "horizon", PortalPassword = "password123", IsPortalActive = false },
                new Customer { Name = "معرض الصحراء الذهبية", Phone = "+968 9345 6789", Email = "goldendesert@cars.om", CivilId = "CR-998877", Address = "مسقط، الحيل", TotalPaid = 0, TotalBalance = 0, CustomerCode = "CUST-005", PortalUsername = "golden", PortalPassword = "password123", IsPortalActive = true },
                new Customer { Name = "خالد الوهيبي", Phone = "+968 9456 7890", Email = "khalid.w@yahoo.com", CivilId = "345678901", Address = "صور", TotalPaid = 0, TotalBalance = 0, CustomerCode = "CUST-006", PortalUsername = "khalid", PortalPassword = "password123", IsPortalActive = true },
                new Customer { Name = "أبراج مسقط للتجارة", Phone = "+968 9567 8901", Email = "towers@muscat.om", CivilId = "CR-445566", Address = "مسقط، روي", TotalPaid = 0, TotalBalance = 0, CustomerCode = "CUST-007", PortalUsername = "abraj", PortalPassword = "password123", IsPortalActive = true },
                new Customer { Name = "يعقوب الشحي", Phone = "+968 9678 9012", Email = "yaqoub@gmail.com", CivilId = "567890123", Address = "خصب", TotalPaid = 0, TotalBalance = 0, CustomerCode = "CUST-008", PortalUsername = "yaqoub", PortalPassword = "password123", IsPortalActive = true },
                new Customer { Name = "الفارس لاستيراد المركبات", Phone = "+968 9789 0123", Email = "alfares@import.om", CivilId = "CR-223344", Address = "البريمي", TotalPaid = 0, TotalBalance = 0, CustomerCode = "CUST-009", PortalUsername = "alfares", PortalPassword = "password123", IsPortalActive = true },
                new Customer { Name = "مريم الزدجالي", Phone = "+968 9890 1234", Email = "maryam.z@gmail.com", CivilId = "789012345", Address = "مسقط، الخوير", TotalPaid = 0, TotalBalance = 0, CustomerCode = "CUST-010", PortalUsername = "maryam", PortalPassword = "password123", IsPortalActive = true }
            };
            context.Customers.AddRange(customers);
            context.SaveChanges();

        
            var cars = new List<Car>
            {
                // Status 1: تم الشراء (4 سيارات)
                new Car { VIN = "1G1RC6E45EU112233", Make = "Chevrolet", Model = "Tahoe", Year = 2023, Color = "Black", PurchasePrice = 45000, InternalCode = "AUTO-24-001", StatusId = 1, CustomerId = customers[0].Id, AuctionId = auctions[0].Id, AddDate = DateTime.Now.AddDays(-2), VehicleShape = "دفع رباعي (SUV)" },
                new Car { VIN = "5YJSA1E45HF334455", Make = "Tesla", Model = "Model S", Year = 2022, Color = "White", PurchasePrice = 38000, InternalCode = "AUTO-24-002", StatusId = 1, CustomerId = customers[1].Id, AuctionId = auctions[1].Id, AddDate = DateTime.Now.AddDays(-3), VehicleShape = "صالون (Sedan)" },
                new Car { VIN = "WBAJB1C54GD667788", Make = "BMW", Model = "X5", Year = 2021, Color = "Blue", PurchasePrice = 32000, InternalCode = "AUTO-24-003", StatusId = 1, CustomerId = customers[4].Id, AuctionId = auctions[2].Id, AddDate = DateTime.Now.AddDays(-1), VehicleShape = "دفع رباعي (SUV)" },
                new Car { VIN = "1FMCU0GX3JU556611", Make = "Ford", Model = "Mustang", Year = 2023, Color = "Red", PurchasePrice = 28000, InternalCode = "AUTO-24-004", StatusId = 1, CustomerId = customers[5].Id, AuctionId = auctions[0].Id, AddDate = DateTime.Now.AddDays(-4), VehicleShape = "كوبيه (Coupe)" },
                
                // Status 2: في المستودع (6 سيارات)
                new Car { VIN = "2T1BURHE3FC556677", Make = "Toyota", Model = "Highlander", Year = 2020, Color = "Silver", PurchasePrice = 25000, InternalCode = "AUTO-24-005", StatusId = 2, CustomerId = customers[2].Id, AuctionId = auctions[1].Id, CurrentLocation = "NJ Warehouse", AddDate = DateTime.Now.AddDays(-12), VehicleShape = "دفع رباعي (SUV)" },
                new Car { VIN = "3VW4T1BF2JG778899", Make = "Volkswagen", Model = "Tiguan", Year = 2019, Color = "Grey", PurchasePrice = 18500, InternalCode = "AUTO-24-006", StatusId = 2, CustomerId = customers[3].Id, AuctionId = auctions[2].Id, CurrentLocation = "TX Warehouse", AddDate = DateTime.Now.AddDays(-15), VehicleShape = "دفع رباعي (SUV)" },
                new Car { VIN = "JTDKN3DP2H1223344", Make = "Lexus", Model = "LX 600", Year = 2023, Color = "White", PurchasePrice = 85000, InternalCode = "AUTO-24-007", StatusId = 2, CustomerId = customers[0].Id, AuctionId = auctions[0].Id, CurrentLocation = "CA Warehouse", AddDate = DateTime.Now.AddDays(-10), VehicleShape = "دفع رباعي (SUV)" },
                new Car { VIN = "KNDJX3AE4G7889900", Make = "Kia", Model = "Telluride", Year = 2022, Color = "Black", PurchasePrice = 22000, InternalCode = "AUTO-24-008", StatusId = 2, CustomerId = customers[6].Id, AuctionId = auctions[1].Id, CurrentLocation = "NJ Warehouse", AddDate = DateTime.Now.AddDays(-14), VehicleShape = "دفع رباعي (SUV)" },
                new Car { VIN = "1C4PJLCX0KD445566", Make = "Jeep", Model = "Wrangler", Year = 2021, Color = "Green", PurchasePrice = 27000, InternalCode = "AUTO-24-009", StatusId = 2, CustomerId = customers[7].Id, AuctionId = auctions[3].Id, CurrentLocation = "TX Warehouse", AddDate = DateTime.Now.AddDays(-11), VehicleShape = "دفع رباعي (SUV)" },
                new Car { VIN = "WA1L2AFY5MD778899", Make = "Audi", Model = "A6", Year = 2020, Color = "Black", PurchasePrice = 31000, InternalCode = "AUTO-24-010", StatusId = 2, CustomerId = customers[8].Id, AuctionId = auctions[4].Id, CurrentLocation = "مستودع الشارقة", AddDate = DateTime.Now.AddDays(-8), VehicleShape = "صالون (Sedan)" },
                
                // Status 3: الشحن البحري (8 سيارات)
                new Car { VIN = "JN1AZ4EH9KM112233", Make = "Nissan", Model = "Patrol", Year = 2023, Color = "Pearl White", PurchasePrice = 65000, InternalCode = "AUTO-24-011", StatusId = 3, CustomerId = customers[0].Id, AuctionId = auctions[0].Id, AddDate = DateTime.Now.AddDays(-30), VehicleShape = "دفع رباعي (SUV)" },
                new Car { VIN = "JHM42Z150KC010203", Make = "Honda", Model = "Accord", Year = 2021, Color = "Silver", PurchasePrice = 19000, InternalCode = "AUTO-24-012", StatusId = 3, CustomerId = customers[1].Id, AuctionId = auctions[1].Id, AddDate = DateTime.Now.AddDays(-28), VehicleShape = "صالون (Sedan)" },
                new Car { VIN = "1G1YB2D47H5112233", Make = "Chevrolet", Model = "Corvette", Year = 2018, Color = "Yellow", PurchasePrice = 42000, InternalCode = "AUTO-24-013", StatusId = 3, CustomerId = customers[2].Id, AuctionId = auctions[2].Id, AddDate = DateTime.Now.AddDays(-35), VehicleShape = "كوبيه (Coupe)" },
                new Car { VIN = "WP0AB2A90KS123456", Make = "Porsche", Model = "911", Year = 2019, Color = "Red", PurchasePrice = 95000, InternalCode = "AUTO-24-014", StatusId = 3, CustomerId = customers[4].Id, AuctionId = auctions[3].Id, AddDate = DateTime.Now.AddDays(-25), VehicleShape = "كوبيه (Coupe)" },
                new Car { VIN = "SALWA2CE6KH001122", Make = "Land Rover", Model = "Range Rover", Year = 2022, Color = "Black", PurchasePrice = 75000, InternalCode = "AUTO-24-015", StatusId = 3, CustomerId = customers[5].Id, AuctionId = auctions[0].Id, AddDate = DateTime.Now.AddDays(-22), VehicleShape = "دفع رباعي (SUV)" },
                new Car { VIN = "4T1B11HK5JU112233", Make = "Toyota", Model = "Camry", Year = 2021, Color = "White", PurchasePrice = 18000, InternalCode = "AUTO-24-016", StatusId = 3, CustomerId = customers[6].Id, AuctionId = auctions[1].Id, AddDate = DateTime.Now.AddDays(-40), VehicleShape = "صالون (Sedan)" },
                new Car { VIN = "1FTEW1EP8KK334455", Make = "Ford", Model = "F-150", Year = 2020, Color = "Blue", PurchasePrice = 35000, InternalCode = "AUTO-24-017", StatusId = 3, CustomerId = customers[8].Id, AuctionId = auctions[2].Id, AddDate = DateTime.Now.AddDays(-29), VehicleShape = "بيك أب (Pickup)" },
                new Car { VIN = "KM8K33A40JU556677", Make = "Hyundai", Model = "Sonata", Year = 2019, Color = "Grey", PurchasePrice = 14000, InternalCode = "AUTO-24-018", StatusId = 3, CustomerId = customers[9].Id, AuctionId = auctions[4].Id, AddDate = DateTime.Now.AddDays(-20), VehicleShape = "صالون (Sedan)" },
                
                // Status 4: واصلة الميناء (4 سيارات)
                new Car { VIN = "JTDKN3DP2H1223344", Make = "Lexus", Model = "ES 350", Year = 2020, Color = "Black", PurchasePrice = 32000, InternalCode = "AUTO-24-019", StatusId = 4, CustomerId = customers[3].Id, AuctionId = auctions[0].Id, CurrentLocation = "ميناء صلالة", AddDate = DateTime.Now.AddDays(-55), VehicleShape = "صالون (Sedan)" },
                new Car { VIN = "1FMCU0GX3JU556677", Make = "Ford", Model = "Escape", Year = 2018, Color = "Silver", PurchasePrice = 11000, InternalCode = "AUTO-24-020", StatusId = 4, CustomerId = customers[7].Id, AuctionId = auctions[1].Id, CurrentLocation = "ميناء صحار", AddDate = DateTime.Now.AddDays(-60), VehicleShape = "دفع رباعي (SUV)" },
                new Car { VIN = "WBA53AB02H5112233", Make = "BMW", Model = "530i", Year = 2019, Color = "White", PurchasePrice = 29000, InternalCode = "AUTO-24-021", StatusId = 4, CustomerId = customers[0].Id, AuctionId = auctions[2].Id, CurrentLocation = "ميناء صحار", AddDate = DateTime.Now.AddDays(-50), VehicleShape = "صالون (Sedan)" },
                new Car { VIN = "WMW13BB03H2233445", Make = "MINI", Model = "Cooper", Year = 2017, Color = "Red", PurchasePrice = 12000, InternalCode = "AUTO-24-022", StatusId = 4, CustomerId = customers[9].Id, AuctionId = auctions[3].Id, CurrentLocation = "ميناء صلالة", AddDate = DateTime.Now.AddDays(-48), VehicleShape = "صالون (Sedan)" },
                
                // Status 5: مخلصة ومسلمة (3 سيارات)
                new Car { VIN = "5N1AR2MN1JC112233", Make = "Nissan", Model = "Armada", Year = 2021, Color = "Silver", PurchasePrice = 38000, InternalCode = "AUTO-24-023", StatusId = 5, CustomerId = customers[4].Id, AuctionId = auctions[0].Id, CurrentLocation = "مسلمة للعميل", AddDate = DateTime.Now.AddDays(-90), VehicleShape = "دفع رباعي (SUV)" },
                new Car { VIN = "1C4HJXEN8JW334455", Make = "Jeep", Model = "Gladiator", Year = 2019, Color = "Grey", PurchasePrice = 28000, InternalCode = "AUTO-24-024", StatusId = 5, CustomerId = customers[2].Id, AuctionId = auctions[1].Id, CurrentLocation = "معرض العميل", AddDate = DateTime.Now.AddDays(-85), VehicleShape = "دفع رباعي (SUV)" },
                new Car { VIN = "2T1BR32EXHC556677", Make = "Toyota", Model = "Corolla", Year = 2018, Color = "White", PurchasePrice = 13000, InternalCode = "AUTO-24-025", StatusId = 5, CustomerId = customers[8].Id, AuctionId = auctions[2].Id, CurrentLocation = "مسلمة للعميل", AddDate = DateTime.Now.AddDays(-70), VehicleShape = "صالون (Sedan)" }
            };
            context.Cars.AddRange(cars);
            context.SaveChanges();

     
            var shipments = new List<Shipment>
            {
                new Shipment { ContainerNumber = "MSKU8877665", BookingNumber = "BKG-102030", BillOfLading = "BL-5544", ShippingLine = "Maersk", LoadingPortId = locations[0].Id, DischargePortId = locations[6].Id, ExpectedTimeOfDeparture = DateTime.Now.AddDays(-20), ExpectedTimeOfArrival = DateTime.Now.AddDays(10), Status = "Sailed" }, // من NJ لصلالة
                new Shipment { ContainerNumber = "TGHU5544332", BookingNumber = "BKG-908070", BillOfLading = "BL-9988", ShippingLine = "MSC", LoadingPortId = locations[1].Id, DischargePortId = locations[7].Id, ExpectedTimeOfDeparture = DateTime.Now.AddDays(-45), ExpectedTimeOfArrival = DateTime.Now.AddDays(-5), Status = "Arrived" }, // من SAV لصحار
                new Shipment { ContainerNumber = "CMAU1122334", BookingNumber = "BKG-334455", BillOfLading = "BL-1122", ShippingLine = "CMA CGM", LoadingPortId = locations[2].Id, DischargePortId = locations[6].Id, ExpectedTimeOfDeparture = DateTime.Now.AddDays(-10), ExpectedTimeOfArrival = DateTime.Now.AddDays(25), Status = "Sailed" }, // من LAX لصلالة
                new Shipment { ContainerNumber = "HLCU9988776", BookingNumber = "BKG-667788", BillOfLading = "BL-3344", ShippingLine = "Hapag-Lloyd", LoadingPortId = locations[0].Id, DischargePortId = locations[7].Id, ExpectedTimeOfDeparture = DateTime.Now.AddDays(2), ExpectedTimeOfArrival = DateTime.Now.AddDays(32), Status = "Loading" }, // قيد التحميل في NJ
                new Shipment { ContainerNumber = "UASC4455667", BookingNumber = "BKG-221100", BillOfLading = "BL-5566", ShippingLine = "UASC", LoadingPortId = locations[8].Id, DischargePortId = locations[7].Id, ExpectedTimeOfDeparture = DateTime.Now.AddDays(-5), ExpectedTimeOfArrival = DateTime.Now.AddDays(2), Status = "Sailed" } // من جبل علي لصحار
            };
            context.Shipments.AddRange(shipments);
            context.SaveChanges();

            // ربط السيارات المبحرة بالشحنات
            cars[10].ShipmentId = shipments[0].Id; // Patrol (Sailed)
            cars[11].ShipmentId = shipments[0].Id; // Accord (Sailed)
            cars[12].ShipmentId = shipments[0].Id; // Corvette (Sailed)

            cars[18].ShipmentId = shipments[1].Id; // ES 350 (Arrived)
            cars[19].ShipmentId = shipments[1].Id; // Escape (Arrived)
            cars[20].ShipmentId = shipments[1].Id; // 530i (Arrived)

            cars[13].ShipmentId = shipments[2].Id; // Porsche (Sailed)
            cars[14].ShipmentId = shipments[2].Id; // Range Rover (Sailed)

            cars[17].ShipmentId = shipments[4].Id; // Sonata (Sailed from UAE)

            context.SaveChanges();

        
            var dispatchOrders = new List<DispatchOrder>
            {
                new DispatchOrder { OrderNumber = "DIS-2024-001", CarId = cars[4].Id, TransporterId = transporters[0].Id, OriginLocationId = locations[0].Id, DestinationLocationId = locations[3].Id, TowingFee = 350, DispatchDate = DateTime.Now.AddDays(-10), Status = "Delivered", DriverNotes = "السيارة بحالة جيدة، تم تسليم المفتاح." },
                new DispatchOrder { OrderNumber = "DIS-2024-002", CarId = cars[5].Id, TransporterId = transporters[1].Id, OriginLocationId = locations[1].Id, DestinationLocationId = locations[4].Id, TowingFee = 450, DispatchDate = DateTime.Now.AddDays(-12), Status = "Delivered", DriverNotes = "يوجد خدش بالصدام الأمامي." },
                new DispatchOrder { OrderNumber = "DIS-2024-003", CarId = cars[6].Id, TransporterId = transporters[3].Id, OriginLocationId = locations[2].Id, DestinationLocationId = locations[5].Id, TowingFee = 250, DispatchDate = DateTime.Now.AddDays(-8), Status = "Delivered", DriverNotes = "السيارة لا تعمل (Non-runner)." },
                new DispatchOrder { OrderNumber = "DIS-2024-004", CarId = cars[0].Id, TransporterId = transporters[0].Id, OriginLocationId = locations[0].Id, DestinationLocationId = locations[3].Id, TowingFee = 300, DispatchDate = DateTime.Now.AddDays(-1), Status = "Assigned", DriverNotes = "بانتظار السائق للتحميل من ساحة كوبارت." },
                new DispatchOrder { OrderNumber = "DIS-2024-005", CarId = cars[1].Id, TransporterId = transporters[1].Id, OriginLocationId = locations[1].Id, DestinationLocationId = locations[4].Id, TowingFee = 400, DispatchDate = DateTime.Now.AddDays(-2), Status = "PickedUp", DriverNotes = "تم التحميل وهي في الطريق للمستودع." },
                new DispatchOrder { OrderNumber = "DIS-2024-006", CarId = cars[9].Id, TransporterId = transporters[4].Id, OriginLocationId = locations[8].Id, DestinationLocationId = locations[9].Id, TowingFee = 150, DispatchDate = DateTime.Now.AddDays(-6), Status = "Delivered", DriverNotes = "نقل من جبل علي للشارقة." }
            };
            context.DispatchOrders.AddRange(dispatchOrders);
            context.SaveChanges();

     
            var invoices = new List<Invoice>();
            var receipts = new List<PaymentReceipt>();
            Random rand = new Random();

            foreach (var car in cars)
            {
                decimal carPrice = car.PurchasePrice;
                decimal auctionFee = rand.Next(400, 900);
                decimal landFreight = rand.Next(250, 600);
                decimal seaFreight = (car.StatusId >= 3) ? rand.Next(900, 1400) : 0;
                decimal customs = (carPrice + seaFreight) * 0.05m; // 5% جمارك
                decimal admin = 150; // عمولة ثابتة

                decimal totalUsd = carPrice + auctionFee + landFreight + seaFreight + customs + admin;

                var inv = new Invoice
                {
                    InvoiceNumber = "INV-" + car.InternalCode.Replace("AUTO-24-", "24"),
                    IssueDate = (car.AddDate ?? DateTime.Now).AddDays(1), // الفاتورة بعد يوم من الإضافة
                    CustomerId = car.CustomerId,
                    CarId = car.Id,
                    CarPrice = carPrice,
                    AuctionFees = auctionFee,
                    LandFreight = landFreight,
                    SeaFreight = seaFreight,
                    CustomsFees = customs,
                    AdminFees = admin,
                    TotalAmount = totalUsd
                };
                invoices.Add(inv);

                // إضافة المديونية للعميل
                var cust = customers.First(c => c.Id == car.CustomerId);
                cust.TotalBalance += totalUsd;
            }
            context.Invoices.AddRange(invoices);
            context.SaveChanges();

            // إضافة سندات قبض (دفعات من العملاء)
            int recCounter = 1;
            foreach (var cust in customers)
            {
                if (cust.TotalBalance > 0)
                {
                    // العميل يدفع حوالي 50% إلى 90% من ديونه
                    decimal payAmount = Math.Round(cust.TotalBalance * (decimal)(rand.Next(50, 90) / 100.0), 2);

                    var rec = new PaymentReceipt
                    {
                        ReceiptNumber = "REC-10" + recCounter.ToString("D3"),
                        CustomerId = cust.Id,
                        Amount = payAmount,
                        PaymentDate = DateTime.Now.AddDays(-rand.Next(5, 40)),
                        PaymentMethod = recCounter % 2 == 0 ? "Bank Transfer" : "Cash",
                        ReferenceNumber = recCounter % 2 == 0 ? "TRX-" + rand.Next(10000, 99999) : "",
                        Notes = "دفعة من الحساب"
                    };
                    receipts.Add(rec);

                    cust.TotalPaid += payAmount;
                    cust.TotalBalance -= payAmount;
                    recCounter++;
                }
            }
            context.PaymentReceipts.AddRange(receipts);
            context.SaveChanges();

            var logs = new List<TrackingLog>();
            var notifications = new List<Notification>();

            foreach (var car in cars)
            {
                // الجميع تم شرائه
                logs.Add(new TrackingLog { CarId = car.Id, Title = "تم الشراء من المزاد", Location = "USA", Description = "تم فوز السيارة بالمزاد وجاري الترتيب للنقل الداخلي.", ProgressPercentage = 10, UpdateDate = (car.AddDate ?? DateTime.Now).AddDays(1) });
                notifications.Add(new Notification { CustomerId = car.CustomerId, Title = "مبروك! تم الشراء", Message = $"تم شراء السيارة {car.Make} {car.Model} بنجاح من المزاد.", Type = "TrackingUpdate", IsRead = true, CreatedAt = (car.AddDate ?? DateTime.Now).AddDays(1) });

                if (car.StatusId >= 2) // وصلت المستودع
                {
                    logs.Add(new TrackingLog { CarId = car.Id, Title = "السيارة في المستودع", Location = car.CurrentLocation ?? "Warehouse", Description = "تم استلام السيارة في مستودع التجميع وتم إرفاق صور الفحص.", ProgressPercentage = 33, UpdateDate = (car.AddDate ?? DateTime.Now).AddDays(6) });
                }

                if (car.StatusId >= 3) // أبحرت
                {
                    logs.Add(new TrackingLog { CarId = car.Id, Title = "تم التحميل وأبحرت السفينة", Location = "المحيط", Description = "تم تحميل السيارة في الحاوية والسفينة في طريقها إلى ميناء الوجهة.", ProgressPercentage = 66, UpdateDate = (car.AddDate ?? DateTime.Now).AddDays(15) });
                    notifications.Add(new Notification { CustomerId = car.CustomerId, Title = "تحديث شحن", Message = $"سيارتك {car.Make} أبحرت وهي الآن في الطريق للبلاد.", Type = "TrackingUpdate", IsRead = false, CreatedAt = (car.AddDate ?? DateTime.Now).AddDays(15) });
                }

                if (car.StatusId >= 4) // وصلت الميناء
                {
                    logs.Add(new TrackingLog { CarId = car.Id, Title = "وصلت إلى الميناء الوجهة", Location = car.CurrentLocation ?? "Port", Description = "وصلت السفينة بسلام وجاري البدء في إجراءات التخليص الجمركي.", ProgressPercentage = 100, UpdateDate = (car.AddDate ?? DateTime.Now).AddDays(45) });
                    notifications.Add(new Notification { CustomerId = car.CustomerId, Title = "تنبيه وصول هام", Message = $"سيارتك {car.Make} وصلت إلى الميناء! يرجى المراجعة لدفع الرسوم المتبقية.", Type = "InvoiceAlert", IsRead = false, CreatedAt = (car.AddDate ?? DateTime.Now).AddDays(45) });
                }
            }
            context.TrackingLogs.AddRange(logs);
            context.Notifications.AddRange(notifications);
            context.SaveChanges();
        }
    }
}