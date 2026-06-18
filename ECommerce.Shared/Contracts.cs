namespace ECommerce.Shared;

public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public string ProductId { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Price { get; init; }
}

public record ReserveInventoryCommand
{
    public Guid OrderId { get; init; }
    public string ProductId { get; init; } = string.Empty;
    public int Quantity { get; init; }
}

public record InventoryReservedEvent
{
    public Guid OrderId { get; init; }
}

public record InventoryReservationFailedEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record ProcessPaymentCommand
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
}

public record PaymentSuccessEvent
{
    public Guid OrderId { get; init; }
}

public record PaymentFailedEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record CancelInventoryReservationCommand
{
    public Guid OrderId { get; init; }
    public string ProductId { get; init; } = string.Empty;
    public int Quantity { get; init; }
}