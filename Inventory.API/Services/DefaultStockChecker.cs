namespace Inventory.API.Services
{
    public class DefaultStockChecker : IStockChecker
    {
        public bool IsProductAvailable(string productId, int quantity)
        {
            return productId != "bad_product";
        }
    }
}