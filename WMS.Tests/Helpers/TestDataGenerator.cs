using Bogus;
using WMS.Domain.Entities;
using WMS.Domain.Enums;

namespace WMS.Tests.Helpers;

/// <summary>
/// Test data generator using Bogus library
/// Provides realistic test data for all entities
/// </summary>
public static class TestDataGenerator
{
    private static int _productCounter = 1;
    private static int _locationCounter = 1;
    private static int _inboundCounter = 1;
    private static int _outboundCounter = 1;

    public static Product GenerateProduct(ProductStatus status = ProductStatus.Active)
    {
        var faker = new Faker();
        var productNumber = _productCounter++;

        return new Product
        {
            Id = Guid.NewGuid(),
            SKU = $"PROD-{productNumber:D4}",
            Name = faker.Commerce.ProductName(),
            Description = faker.Commerce.ProductDescription(),
            Status = status,
            UOM = faker.PickRandom("PCS", "KG", "BOX", "EA"),
            Weight = faker.Random.Decimal(0.1m, 100m),
            Length = faker.Random.Decimal(10m, 200m),
            Width = faker.Random.Decimal(10m, 200m),
            Height = faker.Random.Decimal(10m, 200m),
            Barcode = faker.Commerce.Ean13(),
            Category = faker.Commerce.Categories(1)[0],
            ReorderLevel = faker.Random.Decimal(10m, 100m),
            MaxStockLevel = faker.Random.Decimal(200m, 1000m),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser",
            RowVersion = new byte[8]
        };
    }

    public static Location GenerateLocation(bool isActive = true, decimal capacity = 1000m)
    {
        var faker = new Faker();
        var locationNumber = _locationCounter++;

        return new Location
        {
            Id = Guid.NewGuid(),
            Code = $"LOC-{locationNumber:D4}",
            Name = $"Location {locationNumber}",
            Description = faker.Lorem.Sentence(),
            Zone = faker.PickRandom("A", "B", "C", "D"),
            Aisle = faker.Random.Number(1, 20).ToString("D2"),
            Rack = faker.Random.Number(1, 10).ToString("D2"),
            Shelf = faker.Random.Number(1, 5).ToString("D2"),
            Bin = faker.Random.Number(1, 10).ToString("D2"),
            Capacity = capacity,
            CurrentOccupancy = 0,
            IsActive = isActive,
            LocationType = faker.PickRandom("Storage", "Receiving", "Shipping", "Picking"),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser",
            RowVersion = new byte[8]
        };
    }

    public static Domain.Entities.Inventory GenerateInventory(Guid productId, Guid locationId, 
        decimal quantityOnHand = 100m, decimal quantityReserved = 0m)
    {
        return new Domain.Entities.Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            LocationId = locationId,
            QuantityOnHand = quantityOnHand,
            QuantityReserved = quantityReserved,
            LastStockDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser",
            RowVersion = new byte[8]
        };
    }

    public static Domain.Entities.Inbound GenerateInbound(string supplierName = "Test Supplier")
    {
        var inboundNumber = _inboundCounter++;

        return new Domain.Entities.Inbound
        {
            Id = Guid.NewGuid(),
            InboundNumber = $"IB-{DateTime.UtcNow:yyyyMMdd}-{inboundNumber:D4}",
            ReferenceNumber = $"PO-{inboundNumber:D6}",
            Status = InboundStatus.Pending,
            ExpectedDate = DateTime.UtcNow.AddDays(3),
            SupplierName = supplierName,
            SupplierCode = $"SUP-{inboundNumber:D3}",
            Notes = "Test inbound shipment",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser",
            RowVersion = new byte[8]
        };
    }

    public static InboundItem GenerateInboundItem(Guid inboundId, Guid productId, 
        Guid locationId, decimal expectedQuantity = 100m)
    {
        return new InboundItem
        {
            Id = Guid.NewGuid(),
            InboundId = inboundId,
            ProductId = productId,
            LocationId = locationId,
            ExpectedQuantity = expectedQuantity,
            ReceivedQuantity = 0,
            DamagedQuantity = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser",
            RowVersion = new byte[8]
        };
    }

    public static Domain.Entities.Outbound GenerateOutbound()
    {
        var faker = new Faker();
        var outboundNumber = _outboundCounter++;

        return new Domain.Entities.Outbound
        {
            Id = Guid.NewGuid(),
            OutboundNumber = $"OUT-{DateTime.UtcNow:yyyyMMdd}-{outboundNumber:D4}",
            OrderNumber = $"SO-{outboundNumber:D6}",
            Status = OutboundStatus.Pending,
            OrderDate = DateTime.UtcNow,
            CustomerName = faker.Company.CompanyName(),
            CustomerCode = $"CUST-{outboundNumber:D3}",
            ShippingAddress = faker.Address.FullAddress(),
            Notes = "Test outbound order",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser",
            RowVersion = new byte[8]
        };
    }

    public static OutboundItem GenerateOutboundItem(Guid outboundId, Guid productId, 
        Guid locationId, decimal orderedQuantity = 10m)
    {
        return new OutboundItem
        {
            Id = Guid.NewGuid(),
            OutboundId = outboundId,
            ProductId = productId,
            LocationId = locationId,
            OrderedQuantity = orderedQuantity,
            PickedQuantity = 0,
            ShippedQuantity = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser",
            RowVersion = new byte[8]
        };
    }

    public static Domain.Entities.Payment GeneratePayment(Guid? outboundId = null, 
        PaymentType paymentType = PaymentType.Prepaid, decimal amount = 1000m)
    {
        return new Domain.Entities.Payment
        {
            Id = Guid.NewGuid(),
            PaymentNumber = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 20),
            OutboundId = outboundId,
            PaymentType = paymentType,
            Status = PaymentStatus.Pending,
            Amount = amount,
            Currency = "USD",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser",
            RowVersion = new byte[8]
        };
    }

    public static Domain.Entities.Delivery GenerateDelivery(Guid outboundId, string trackingNumber = "")
    {
        var faker = new Faker();

        return new Domain.Entities.Delivery
        {
            Id = Guid.NewGuid(),
            DeliveryNumber = $"DEL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 20),
            OutboundId = outboundId,
            Status = DeliveryStatus.Pending,
            ShippingAddress = faker.Address.FullAddress(),
            ShippingCity = faker.Address.City(),
            ShippingState = faker.Address.State(),
            ShippingZipCode = faker.Address.ZipCode(),
            ShippingCountry = "USA",
            Carrier = faker.PickRandom("FedEx", "UPS", "DHL", "USPS"),
            TrackingNumber = string.IsNullOrEmpty(trackingNumber) ? faker.Random.AlphaNumeric(16) : trackingNumber,
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser",
            RowVersion = new byte[8]
        };
    }

    public static void ResetCounters()
    {
        _productCounter = 1;
        _locationCounter = 1;
        _inboundCounter = 1;
        _outboundCounter = 1;
    }
}
