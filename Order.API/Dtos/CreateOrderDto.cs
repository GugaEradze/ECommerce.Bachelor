namespace Order.API.Dtos
{
    public record CreateOrderDto(string ProductId, int Quantity, decimal Price);
}