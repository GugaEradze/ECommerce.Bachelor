using MassTransit;
using ECommerce.Shared;
using Inventory.API.Services;

namespace Inventory.API.Consumers
{
    public class ReserveInventoryConsumer : IConsumer<ReserveInventoryCommand>
    {
        private readonly IStockChecker _stockChecker;
        private readonly ILogger<ReserveInventoryConsumer> _logger;

        public ReserveInventoryConsumer(IStockChecker stockChecker, ILogger<ReserveInventoryConsumer> logger)
        {
            _stockChecker = stockChecker;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ReserveInventoryCommand> context)
        {
            try
            {
                var msg = context.Message;

                if (!_stockChecker.IsProductAvailable(msg.ProductId, msg.Quantity))
                {
                    _logger.LogWarning("პროდუქტი არ არის მარაგში: {ProductId}, შეკვეთა: {OrderId}", msg.ProductId, msg.OrderId);
                    await context.Publish(new InventoryReservationFailedEvent
                    {
                        OrderId = msg.OrderId,
                        Reason = "პროდუქტი საწყობში არ არის!"
                    });
                }
                else
                {
                    _logger.LogInformation("ინვენტარი დარეზერვდა შეკვეთისთვის: {OrderId}", msg.OrderId);
                    await context.Publish(new InventoryReservedEvent { OrderId = msg.OrderId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "სისტემური შეცდომა ინვენტარის რეზერვაციისას: {OrderId}", context.Message.OrderId);
                await context.Publish(new InventoryReservationFailedEvent
                {
                    OrderId = context.Message.OrderId,
                    Reason = "სისტემური შეცდომა საწყობთან კავშირისას."
                });
            }
        }
    }
}