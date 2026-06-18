using MassTransit;

namespace Order.API.StateMachines
{
    public class OrderSagaState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; } = string.Empty;


        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}