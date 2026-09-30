using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum PaymentStatus
    {
        Pending, Succeeded, Failed, Cancelled, Refunded, PartiallyRefunded
    }
}
