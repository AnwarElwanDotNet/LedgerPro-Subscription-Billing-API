using System;
using System.Collections.Generic;

namespace Domain.Entity;

public partial class AspNetUser
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int CurrentState { get; set; }

    public Guid? OrganizationId { get; set; }

    public string? UserName { get; set; }

    public string? NormalizedUserName { get; set; }

    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; } = new List<AspNetUserClaim>();

    public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; } = new List<AspNetUserLogin>();

    public virtual ICollection<AspNetUserToken> AspNetUserTokens { get; set; } = new List<AspNetUserToken>();

    public virtual TbOrganization? Organization { get; set; }

    public virtual ICollection<TbAuditLog> TbAuditLogCreatedByNavigations { get; set; } = new List<TbAuditLog>();

    public virtual ICollection<TbAuditLog> TbAuditLogUpdatedByNavigations { get; set; } = new List<TbAuditLog>();

    public virtual ICollection<TbAuditLog> TbAuditLogUsers { get; set; } = new List<TbAuditLog>();

    public virtual ICollection<TbBillingPeriod> TbBillingPeriodCreatedByNavigations { get; set; } = new List<TbBillingPeriod>();

    public virtual ICollection<TbBillingPeriod> TbBillingPeriodUpdatedByNavigations { get; set; } = new List<TbBillingPeriod>();

    public virtual ICollection<TbCancellationRecord> TbCancellationRecordCreatedByNavigations { get; set; } = new List<TbCancellationRecord>();

    public virtual ICollection<TbCancellationRecord> TbCancellationRecordPerformedByNavigations { get; set; } = new List<TbCancellationRecord>();

    public virtual ICollection<TbCancellationRecord> TbCancellationRecordUpdatedByNavigations { get; set; } = new List<TbCancellationRecord>();

    public virtual ICollection<TbGracePeriod> TbGracePeriodCreatedByNavigations { get; set; } = new List<TbGracePeriod>();

    public virtual ICollection<TbGracePeriod> TbGracePeriodUpdatedByNavigations { get; set; } = new List<TbGracePeriod>();

    public virtual ICollection<TbInvoice> TbInvoiceCreatedByNavigations { get; set; } = new List<TbInvoice>();

    public virtual ICollection<TbInvoice> TbInvoiceUpdatedByNavigations { get; set; } = new List<TbInvoice>();

    public virtual ICollection<TbNotification> TbNotificationCreatedByNavigations { get; set; } = new List<TbNotification>();

    public virtual ICollection<TbNotification> TbNotificationUpdatedByNavigations { get; set; } = new List<TbNotification>();

    public virtual ICollection<TbNotification> TbNotificationUsers { get; set; } = new List<TbNotification>();

    public virtual ICollection<TbOrganization> TbOrganizationCreatedByNavigations { get; set; } = new List<TbOrganization>();

    public virtual ICollection<TbOrganization> TbOrganizationUpdatedByNavigations { get; set; } = new List<TbOrganization>();

    public virtual ICollection<TbPaymentAttempt> TbPaymentAttemptCreatedByNavigations { get; set; } = new List<TbPaymentAttempt>();

    public virtual ICollection<TbPaymentAttempt> TbPaymentAttemptUpdatedByNavigations { get; set; } = new List<TbPaymentAttempt>();

    public virtual ICollection<TbPayment> TbPaymentCreatedByNavigations { get; set; } = new List<TbPayment>();

    public virtual ICollection<TbPayment> TbPaymentUpdatedByNavigations { get; set; } = new List<TbPayment>();

    public virtual ICollection<TbPendingPlanChange> TbPendingPlanChangeCreatedByNavigations { get; set; } = new List<TbPendingPlanChange>();

    public virtual ICollection<TbPendingPlanChange> TbPendingPlanChangeUpdatedByNavigations { get; set; } = new List<TbPendingPlanChange>();

    public virtual ICollection<TbPlan> TbPlanCreatedByNavigations { get; set; } = new List<TbPlan>();

    public virtual ICollection<TbPlanPricing> TbPlanPricingCreatedByNavigations { get; set; } = new List<TbPlanPricing>();

    public virtual ICollection<TbPlanPricing> TbPlanPricingUpdatedByNavigations { get; set; } = new List<TbPlanPricing>();

    public virtual ICollection<TbPlan> TbPlanUpdatedByNavigations { get; set; } = new List<TbPlan>();

    public virtual ICollection<TbRefreshToken> TbRefreshTokenCreatedByNavigations { get; set; } = new List<TbRefreshToken>();

    public virtual ICollection<TbRefreshToken> TbRefreshTokenUpdatedByNavigations { get; set; } = new List<TbRefreshToken>();

    public virtual ICollection<TbRefreshToken> TbRefreshTokenUsers { get; set; } = new List<TbRefreshToken>();

    public virtual ICollection<TbRefund> TbRefundCreatedByNavigations { get; set; } = new List<TbRefund>();

    public virtual ICollection<TbRefund> TbRefundUpdatedByNavigations { get; set; } = new List<TbRefund>();

    public virtual ICollection<TbSubscription> TbSubscriptionCreatedByNavigations { get; set; } = new List<TbSubscription>();

    public virtual ICollection<TbSubscription> TbSubscriptionUpdatedByNavigations { get; set; } = new List<TbSubscription>();

    public virtual ICollection<TbSystemConfig> TbSystemConfigCreatedByNavigations { get; set; } = new List<TbSystemConfig>();

    public virtual ICollection<TbSystemConfig> TbSystemConfigUpdatedByNavigations { get; set; } = new List<TbSystemConfig>();

    public virtual ICollection<TbTrial> TbTrialCreatedByNavigations { get; set; } = new List<TbTrial>();

    public virtual ICollection<TbTrial> TbTrialUpdatedByNavigations { get; set; } = new List<TbTrial>();

    public virtual ICollection<TbUserSeat> TbUserSeatCreatedByNavigations { get; set; } = new List<TbUserSeat>();

    public virtual ICollection<TbUserSeat> TbUserSeatUpdatedByNavigations { get; set; } = new List<TbUserSeat>();

    public virtual ICollection<TbUserSeat> TbUserSeatUsers { get; set; } = new List<TbUserSeat>();

    public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();
}
