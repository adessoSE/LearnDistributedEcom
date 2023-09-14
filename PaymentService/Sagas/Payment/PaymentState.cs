namespace PaymentService.Sagas.Payment;

using System;

using MassTransit;

public class PaymentState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }

    public Guid OrderId => CorrelationId;

    public DateTime? PaymentTime { get; set; }

    public string CurrentState { get; set; } = default!;

    public int Version { get; set; }
}
