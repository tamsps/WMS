namespace WMS.Domain.Enums;

public enum ProductStatus
{
    Active = 1,
    Inactive = 0
}

public enum TransactionType
{
    Inbound = 1,
    Outbound = 2,
    Adjustment = 3,
    Return = 4
}

public enum PaymentStatus
{
    Pending = 0,
    Confirmed = 1,
    Failed = 2,
    Cancelled = 3
}

public enum PaymentType
{
    Prepaid = 1,
    COD = 2,
    Postpaid = 3
}

public enum DeliveryStatus
{
    Pending = 0,
    InTransit = 1,
    Delivered = 2,
    Failed = 3,
    Returned = 4,
    Cancelled = 5
}

public enum InboundStatus
{
    Pending = 0,
    Received = 1,
    PutAway = 2,
    Completed = 3,
    Cancelled = 4
}

public enum OutboundStatus
{
    Pending = 0,
    Picking = 1,
    Picked = 2,
    Packed = 3,
    Shipped = 4,
    Cancelled = 5
}
