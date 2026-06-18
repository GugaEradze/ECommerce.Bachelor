using MassTransit;
using ECommerce.Shared;
using Payment.API.Validators;

namespace Payment.API.Consumers
{
    public class ProcessPaymentConsumer : IConsumer<ProcessPaymentCommand>
    {
        private readonly IPaymentValidator _validator;
        private readonly ILogger<ProcessPaymentConsumer> _logger;

        public ProcessPaymentConsumer(IPaymentValidator validator, ILogger<ProcessPaymentConsumer> logger)
        {
            _validator = validator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProcessPaymentCommand> context)
        {
            try
            {
                var msg = context.Message;

                if (!_validator.IsValid(msg.Amount))
                {
                    _logger.LogWarning("გადახდა უარყოფილია OrderId: {OrderId} თანხა: {Amount}", msg.OrderId, msg.Amount);

                    await context.Publish(new PaymentFailedEvent
                    {
                        OrderId = msg.OrderId,
                        Reason = "არასაკმარისი ბალანსი ანგარიშზე!"
                    });
                }
                else
                {
                    _logger.LogInformation("გადახდა წარმატებულია OrderId: {OrderId}", msg.OrderId);
                    await context.Publish(new PaymentSuccessEvent { OrderId = msg.OrderId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "სისტემური შეცდომა გადახდისას OrderId: {OrderId}", context.Message.OrderId);

                await context.Publish(new PaymentFailedEvent
                {
                    OrderId = context.Message.OrderId,
                    Reason = "სისტემური შეცდომა გადახდის დამუშავებისას."
                });
            }
        }
    }
}