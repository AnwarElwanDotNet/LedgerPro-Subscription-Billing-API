using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum SubscriptionStatus
    {
        PendingPayment, PaymentFailed, Active, ActiveUntilPeriodEnd, PastDue, GracePeriod, Suspended, Cancelled, Expired
    }
}
