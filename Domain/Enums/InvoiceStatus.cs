using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum InvoiceStatus 
    {
        Pending, Paid, PaymentFailed, Void, Refunded, PartiallyRefunded
    }
}
