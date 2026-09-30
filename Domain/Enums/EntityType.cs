using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum EntityType
    {
        Organization, Plan, PlanPricing, Trial, Subscription, BillingPeriod, Invoice, Payment,
        PaymentAttempt, Refund, GracePeriod, UserSeat, Notification, 
        PendingPlanChange, CancellationRecord, User, Role, RefreshToken, SystemConfig
    }
}
