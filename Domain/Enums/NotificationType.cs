using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum NotificationType
    {
        TrialStarted, TrialNearingEnd, TrialEnded, InvoiceCreated, PaymentSucceeded, PaymentFailed,
        RenewalApproaching, RenewalSucceeded, GracePeriodStarted,
        SubscriptionSuspended, SubscriptionExpired, CancellationRequested, PlanChanged, RefundProcessed
    }
}
