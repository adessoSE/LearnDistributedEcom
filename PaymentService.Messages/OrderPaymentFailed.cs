﻿namespace PaymentService.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public record OrderPaymentFailed(Guid OrderId)
{
}
