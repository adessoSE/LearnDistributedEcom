namespace PaymentService.Sagas.Payment;

using MassTransit;

using OrderService.Messages;

using PaymentService.Messages;

public class PaymentStateMachine : MassTransitStateMachine<PaymentState>
{
    public Event<OrderCreated> OrderCreatedEvent { get; private set; } = default!;
    public Event<OrderPaymentFailed> PaymentFailedEvent { get; private set; } = default!;
    public Event<OrderPaymentSucceeded> PaymentSucceededEvent { get; private set; } = default!;

    public State PaymentPending { get; private set; } = default!;

    public State PaymentSuccessful { get; private set; } = default!;

    public State PaymentFailed { get; private set; } = default!;

    public PaymentStateMachine()
    {
        InstanceState(state => state.CurrentState);

        Event(() => OrderCreatedEvent, c =>
        {
            c.CorrelateById(context => context.Message.Id);

            c.InsertOnInitial = true;

            c.SetSagaFactory(context => new PaymentState
            {
                CorrelationId = context.Message.Id
            });
        });

        Event(() => PaymentFailedEvent, c => c.CorrelateById(context => context.Message.OrderId));
        Event(() => PaymentSucceededEvent, c => c.CorrelateById(context => context.Message.OrderId));

        Initially(
            When(OrderCreatedEvent)
                .TransitionTo(PaymentPending));

        During(PaymentPending,
            When(PaymentFailedEvent)
                .TransitionTo(PaymentFailed), // TODO: Add Retry behavior
            When(PaymentSucceededEvent)
                .Then(context => context.Saga.PaymentTime = DateTime.UtcNow)
                .TransitionTo(PaymentSuccessful)
                .Finalize());
                
    }
}

