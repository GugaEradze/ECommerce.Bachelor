using MassTransit;
using ECommerce.Shared;

namespace Order.API.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderSagaState>
    {
        public State Submitted { get; private set; } = null!;
        public State Accepted { get; private set; } = null!;
        public State Failed { get; private set; } = null!;

        public Event<OrderCreatedEvent> OrderCreated { get; private set; } = null!;
        public Event<InventoryReservedEvent> InventoryReserved { get; private set; } = null!;
        public Event<InventoryReservationFailedEvent> InventoryReservationFailed { get; private set; } = null!;
        public Event<PaymentSuccessEvent> PaymentSuccess { get; private set; } = null!;
        public Event<PaymentFailedEvent> PaymentFailed { get; private set; } = null!;

        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderCreated, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => InventoryReserved, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => InventoryReservationFailed, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => PaymentSuccess, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => PaymentFailed, x => x.CorrelateById(context => context.Message.OrderId));

            During(Initial,
                When(OrderCreated)
                    .Then(context =>
                    {
                        context.Saga.ProductId = context.Message.ProductId;
                        context.Saga.Quantity = context.Message.Quantity;
                        context.Saga.Price = context.Message.Price;
                    })
                    .TransitionTo(Submitted)
                    .Publish(context => new ReserveInventoryCommand
                    {
                        OrderId = context.Saga.CorrelationId,
                        ProductId = context.Saga.ProductId,
                        Quantity = context.Saga.Quantity
                    }));

            During(Submitted,
                When(InventoryReserved)
                    .Publish(context => new ProcessPaymentCommand
                    {
                        OrderId = context.Saga.CorrelationId,
                        Amount = context.Saga.Quantity * context.Saga.Price
                    }),
                When(InventoryReservationFailed)
                    .TransitionTo(Failed)
                    .Finalize(),
                When(PaymentSuccess)
                    .TransitionTo(Accepted)
                    .Finalize(),
                When(PaymentFailed)
                    .TransitionTo(Failed)
                    .Publish(context => new CancelInventoryReservationCommand
                    {
                        OrderId = context.Saga.CorrelationId,
                        ProductId = context.Saga.ProductId,
                        Quantity = context.Saga.Quantity
                    })
                    .Finalize());
        }
    }
}