namespace Inventory.API.Services
{
    public interface IStockChecker
    {
        bool IsProductAvailable(string productId, int quantity);
    }
}