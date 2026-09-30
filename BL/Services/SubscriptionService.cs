

using AutoMapper;
using BL.Contract;
using BL.Dtos;
using DAL.Contracts;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace BL.Services
{
    public class SubscriptionService :
        BaseService<TbSubscription, SubscriptionDto>,
        ISubscription
    {
        private readonly ITableRepository<TbSubscription> _SubscriptionRepo;
        private readonly ITableRepository<TbOrganization> _OrganizationRepo;
        private readonly ITableRepository<TbPlan> _PlanRepo;
        private readonly ITableRepository<TbInvoice> _InvoiceRepo;
        private readonly ITableRepository<TbNotification> _NotificationRepo;
        private readonly ITableRepository<TbAuditLog> _AuditLogRepo;
        private readonly ITableRepository<TbCancellationRecord> _CancellationRecord;
        private readonly ITableRepository<TbBillingPeriod> _BillingPeriod;
        private readonly ITableRepository<TbTrial> _TrialRepo;
        private readonly ITableRepository<TbSystemConfig> _SystemConfigRepo;
        private readonly ITableRepository<TbGracePeriod> _GracePeriodRepo;
        private readonly ITableRepository<TbPendingPlanChange> _PendingPlanChangeRepo;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public SubscriptionService(
            ITableRepository<TbSubscription> tableRepository,

            IMapper mapper,
            IUserService userService,
            ITableRepository<TbOrganization> organizationRepo, ITableRepository<TbPlan> tableRepository1,
            ITableRepository<TbInvoice> invoiceRepo, ITableRepository<TbNotification> notificationRepo,
            ITableRepository<TbAuditLog> auditLogRepo, ITableRepository<TbCancellationRecord> tableRepository2,
            ITableRepository<TbBillingPeriod> tableRepository3, ITableRepository<TbTrial> tableRepository4,
            ITableRepository<TbSystemConfig>
            tableRepository5, ITableRepository<TbGracePeriod> tableRepository6, ITableRepository<TbPendingPlanChange> tableRepository7)
            : base(tableRepository, mapper, userService)
        {
            _SubscriptionRepo = tableRepository;


            _mapper = mapper;
            _userService = userService;
            _OrganizationRepo = organizationRepo;
            _PlanRepo = tableRepository1;
            _InvoiceRepo = invoiceRepo;
            _NotificationRepo = notificationRepo;
            _AuditLogRepo = auditLogRepo;
            _CancellationRecord = tableRepository2;
            _BillingPeriod = tableRepository3;
            _TrialRepo = tableRepository4;
            _SystemConfigRepo = tableRepository5;
            _GracePeriodRepo = tableRepository6;
            _PendingPlanChangeRepo = tableRepository7;
        }

        public async Task ActivateSubscriptionAsync(Guid invoiceId)
        {
            if (invoiceId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "invoiceId Is Required");
            }

            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            var currentUser =
                await _userService.GetUserByIdAsync(
                    userId.ToString());

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException(
                    "Current user was not found.");
            }

            if (currentUser.Roles == null ||
                currentUser.Roles.Count == 0)
            {
                throw new UnauthorizedAccessException(
                    "Undefined User Role");
            }

            if (!currentUser.Roles.Contains("Platform Admin") &&
                !currentUser.Roles.Contains("Customer Admin"))
            {
                throw new UnauthorizedAccessException(
                    "User Can Not Activate Subscription");
            }

            if (currentUser.Roles.Contains("Customer Admin") &&
                (currentUser.OrganizationId == null ||
                 currentUser.OrganizationId == Guid.Empty))
            {
                throw new UnauthorizedAccessException(
                    "Organization is required");
            }

            var correlationId = Guid.NewGuid();

            await _SubscriptionRepo.ExecuteInTransactionAsync(
                async () =>
                {
                    var invoice = await _InvoiceRepo
                        .GetTable()
                        .Include(a => a.Payment)
                        .Include(a => a.Subscription)
                        .Where(a =>
                            a.Id == invoiceId &&
                            a.CurrentState > 0)
                        .FirstOrDefaultAsync();

                    if (invoice == null)
                    {
                        throw new UnauthorizedAccessException(
                            "Invoice Undefined");
                    }

                   
                    if (currentUser.Roles.Contains("Customer Admin") &&
                        invoice.OrganizationId != currentUser.OrganizationId)
                    {
                        throw new UnauthorizedAccessException(
                            "Invoice Does Not Belong To Current Organization");
                    }

                   
                    if (invoice.Status == InvoiceStatus.Paid &&
                        invoice.Subscription != null &&
                        invoice.Subscription.Status == SubscriptionStatus.Active)
                    {
                        return;
                    }

                    if (invoice.InvoiceType != InvoiceType.Initial)
                    {
                        throw new UnauthorizedAccessException(
                            "Only Initial Invoice Can Activate Subscription");
                    }

                    if (invoice.Subscription == null)
                    {
                        throw new UnauthorizedAccessException(
                            "Subscription Undefined");
                    }

                    var subscription = invoice.Subscription;

                    if (subscription.CurrentState <= 0)
                    {
                        throw new UnauthorizedAccessException(
                            "Subscription Undefined");
                    }

                    if (subscription.OrganizationId != invoice.OrganizationId)
                    {
                        throw new UnauthorizedAccessException(
                            "Subscription Does Not Belong To Invoice Organization");
                    }

                    if (subscription.Status != SubscriptionStatus.PendingPayment)
                    {
                        throw new UnauthorizedAccessException(
                            "Subscription Is Not Pending Payment");
                    }

                   
                    if (invoice.Payment == null)
                    {
                        throw new UnauthorizedAccessException(
                            "Successful Payment Is Required");
                    }

                    if (invoice.Payment.Status != PaymentStatus.Succeeded)
                    {
                        throw new UnauthorizedAccessException(
                            "Payment Is Not Successful");
                    }

                  
                    if (invoice.Payment.Amount != invoice.Amount)
                    {
                        throw new UnauthorizedAccessException(
                            "Payment Amount Does Not Match Invoice Amount");
                    }

                    if (!string.Equals(
                            invoice.Payment.Currency,
                            invoice.Currency,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        throw new UnauthorizedAccessException(
                            "Payment Currency Does Not Match Invoice Currency");
                    }

                    var activationTime = DateTime.UtcNow;

                    var periodStart = activationTime;

                    var periodEnd =
                        periodStart.AddDays(invoice.Duration);

                    var existingBillingPeriod =
                        await _BillingPeriod
                            .GetTable()
                            .Where(a =>
                                a.SubscriptionId == subscription.Id &&
                                a.CurrentState > 0)
                            .FirstOrDefaultAsync();

                    if (existingBillingPeriod != null)
                    {
                        throw new UnauthorizedAccessException(
                            "Billing Period Already Exists");
                    }

                    var billingPeriod = new TbBillingPeriod
                    {
                        Id = Guid.NewGuid(),

                        SubscriptionId = subscription.Id,
                        PlanId = invoice.PlanId,

                        Start = periodStart,
                        End = periodEnd,

                        Amount = invoice.Amount,
                        Currency = invoice.Currency,

                        Status = BillingPeriodStatus.Active
                    };

                    await _BillingPeriod.AddAsync(
                        billingPeriod,
                        userId);

                   
                    invoice.Status = InvoiceStatus.Paid;

                    invoice.BillingPeriodId =
                        billingPeriod.Id;

                    invoice.StartDate =
                        periodStart;

                    invoice.EndDate =
                        periodEnd;

                    await _InvoiceRepo.UpdateAsync(
                        invoice,
                        userId);

                  
                    subscription.Status =
                        SubscriptionStatus.Active;

                    subscription.StartedAt =
                        activationTime;

                    subscription.CurrentPeriodStart =
                        periodStart;

                    subscription.CurrentPeriodEnd =
                        periodEnd;

                    subscription.PlanId =
                        invoice.PlanId;

                    await _SubscriptionRepo.UpdateAsync(
                        subscription,
                        userId);

                   
                    await _AuditLogRepo.AddAsync(
                        new TbAuditLog
                        {
                            Id = Guid.NewGuid(),

                            UserId = userId,

                            Action =
                                "Activate Subscription",

                            Timestamp =
                                activationTime,

                            EntityType =
                                EntityType.Subscription,

                            EntityId =
                                subscription.Id,

                            Metadata =
                                $"InvoiceId: {invoice.Id}; " +
                                $"PaymentId: {invoice.Payment.Id}",

                            CorrelationId =
                                correlationId
                        },
                        userId);

                    await _NotificationRepo.AddAsync(
                        new TbNotification
                        {
                            Id = Guid.NewGuid(),

                            OrganizationId =
                                subscription.OrganizationId,

                            UserId =
                                userId,

                            Description =
                                "Subscription payment succeeded and subscription activated",

                            RelatedEntityType =
                                EntityType.Subscription,

                            RelatedEntityId =
                                subscription.Id,

                            IsRead =
                                false,

                            Type =
                                NotificationType.PaymentSucceeded
                        },
                        userId);
                },
                System.Data.IsolationLevel.Serializable);
        }
        public async Task ApplySuccessfulUpgradeAsync(Guid invoiceId)
        {
            if (invoiceId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "invoiceId Is Required");
            }

            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            await _SubscriptionRepo.ExecuteInTransactionAsync(
                async () =>
                {
                    var invoice = await _InvoiceRepo
                        .GetTable()
                        .Include(a => a.Payment)
                        .Include(a => a.Subscription)
                        .Include(a => a.BillingPeriod)
                        .Where(a =>
                            a.Id == invoiceId &&
                            a.CurrentState > 0)
                        .FirstOrDefaultAsync();

                    if (invoice == null)
                    {
                        throw new UnauthorizedAccessException(
                            "Invoice Undefined");
                    }

                    if (invoice.InvoiceType != InvoiceType.Proration)
                    {
                        throw new UnauthorizedAccessException(
                            "Only Proration Invoice Can Apply Successful Upgrade");
                    }

                    if (invoice.Subscription == null)
                    {
                        throw new UnauthorizedAccessException(
                            "Subscription Undefined");
                    }

                    var subscription = invoice.Subscription;

                    if (subscription.CurrentState <= 0)
                    {
                        throw new UnauthorizedAccessException(
                            "Subscription Undefined");
                    }

                   
                    if (!subscription.CurrentPeriodStart.HasValue ||
                        !subscription.CurrentPeriodEnd.HasValue)
                    {
                        throw new UnauthorizedAccessException(
                            "Current Billing Period Is Required");
                    }

                   
                    if (invoice.BillingPeriod == null)
                    {
                        throw new UnauthorizedAccessException(
                            "Billing Period Undefined");
                    }

                   
                    if (invoice.BillingPeriod.SubscriptionId != subscription.Id)
                    {
                        throw new UnauthorizedAccessException(
                            "Invoice Billing Period Does Not Belong To Subscription");
                    }

                   
                    if (invoice.BillingPeriod.Start !=
                            subscription.CurrentPeriodStart.Value ||
                        invoice.BillingPeriod.End !=
                            subscription.CurrentPeriodEnd.Value)
                    {
                        throw new UnauthorizedAccessException(
                            "Invoice Billing Period Is Not The Current Billing Period");
                    }

                   
                    if (invoice.Status == InvoiceStatus.Paid &&
                        subscription.PlanId == invoice.PlanId)
                    {
                        return;
                    }

                  
                    if (subscription.Status != SubscriptionStatus.Active)
                    {
                        throw new UnauthorizedAccessException(
                            "Subscription Is Not Active");
                    }

                   
                    if (invoice.Payment == null)
                    {
                        throw new UnauthorizedAccessException(
                            "Successful Payment Is Required");
                    }

                    if (invoice.Payment.Status != PaymentStatus.Succeeded)
                    {
                        throw new UnauthorizedAccessException(
                            "Payment Is Not Successful");
                    }

                    
                    if (invoice.Payment.Amount != invoice.Amount)
                    {
                        throw new UnauthorizedAccessException(
                            "Payment Amount Does Not Match Invoice Amount");
                    }

                    if (!string.Equals(
                            invoice.Payment.Currency,
                            invoice.Currency,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        throw new UnauthorizedAccessException(
                            "Payment Currency Does Not Match Invoice Currency");
                    }

                  
                    if (invoice.Status != InvoiceStatus.Pending)
                    {
                        throw new UnauthorizedAccessException(
                            "Invoice Is Not Pending");
                    }

                   
                    subscription.PlanId = invoice.PlanId;

                    await _SubscriptionRepo.UpdateAsync(
                        subscription,
                        userId);

                   
                    invoice.Status = InvoiceStatus.Paid;

                    await _InvoiceRepo.UpdateAsync(
                        invoice,
                        userId);

                  
                    await _NotificationRepo.AddAsync(
                        new TbNotification
                        {
                            Id = Guid.NewGuid(),
                            OrganizationId = subscription.OrganizationId,
                            UserId = userId,
                            Description =
                                "Subscription plan upgraded successfully",
                            RelatedEntityType = EntityType.Subscription,
                            RelatedEntityId = subscription.Id,
                            IsRead = false,
                            Type = NotificationType.PlanChanged
                        },
                        userId);

                   
                    await _AuditLogRepo.AddAsync(
                        new TbAuditLog
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            Action = "Apply Successful Upgrade",
                            Timestamp = DateTime.UtcNow,
                            EntityType = EntityType.Subscription,
                            EntityId = subscription.Id,
                            Metadata =
                                $"InvoiceId: {invoice.Id}; NewPlanId: {invoice.PlanId}; PaymentId: {invoice.Payment.Id}",
                            CorrelationId = Guid.NewGuid()
                        },
                        userId);
                },
                System.Data.IsolationLevel.Serializable);
        }
        public async Task CancelSubscriptionAsync(Guid subscriptionId, string reason = null)
        {
            if (subscriptionId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "subscriptionId Is Required");
            }

            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            var currentUser =
                await _userService.GetUserByIdAsync(userId.ToString());

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException(
                    "Current user was not found.");
            }

            var userRole = currentUser.Roles;

            if (!userRole.Contains("Customer Admin"))
            {
                throw new UnauthorizedAccessException(
                    "User Role Can Not Cancel Subscription");
            }

            if (currentUser.OrganizationId == null ||
                currentUser.OrganizationId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "Organization is required");
            }

            var organizationId = currentUser.OrganizationId.Value;

            var correlationId = Guid.NewGuid();

            await _SubscriptionRepo.ExecuteInTransactionAsync(
                async () =>
                {
                    var subscription =
                        await _SubscriptionRepo
                            .GetTable()
                            .Where(a =>
                                a.Id == subscriptionId &&
                                a.OrganizationId == organizationId &&
                                a.CurrentState > 0 &&
                                a.Status == SubscriptionStatus.Active)
                            .FirstOrDefaultAsync();

                    if (subscription == null)
                    {
                        throw new UnauthorizedAccessException(
                            "Subscription Undefined");
                    }

                    if (subscription.CurrentPeriodEnd == null)
                    {
                        throw new UnauthorizedAccessException(
                            "Current Period End is required");
                    }

                    subscription.Status =
                        SubscriptionStatus.ActiveUntilPeriodEnd;

                    await _SubscriptionRepo.UpdateAsync(
                        subscription,
                        userId);

                    await _CancellationRecord.AddAsync(
                        new TbCancellationRecord
                        {
                            OrganizationId = organizationId,
                            SubscriptionId = subscription.Id,
                            PerformedBy = userId,
                            PerformedAt = DateTime.UtcNow,
                            EffectiveDate = subscription.CurrentPeriodEnd.Value,
                            Reason = reason,
                            Type = CancellationType.EndOfPeriod,
                            AccessEndedImmediately = false,
                            RefundInitiated = false
                        },
                        userId);

                    await _AuditLogRepo.AddAsync(
                        new TbAuditLog
                        {
                            UserId = userId,
                            Action = "Cancel Subscription",
                            Timestamp = DateTime.UtcNow,
                            EntityType = EntityType.Subscription,
                            EntityId = subscription.Id,
                            Metadata = "Cancellation Type: EndOfPeriod",
                            CorrelationId = correlationId
                        },
                        userId);

                    await _NotificationRepo.AddAsync(
                        new TbNotification
                        {
                            OrganizationId = organizationId,
                            UserId = userId,
                            Description =
                                "Subscription cancellation requested",
                            RelatedEntityType = EntityType.Subscription,
                            RelatedEntityId = subscription.Id,
                            IsRead = false,
                            Type = NotificationType.CancellationRequested
                        },
                        userId);
                },
                System.Data.IsolationLevel.Serializable);
        }
        public async Task CancelSubscriptionImmediatelyAsync(Guid subscriptionId, string reason = null)
        {
            if (subscriptionId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                "subscriptionId Is Required");
            }

            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            var currentUser =
                await _userService.GetUserByIdAsync(userId.ToString());

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException(
                    "Current user was not found.");
            }

            var userRole = currentUser.Roles;
            if (!userRole.Contains("Platform Admin"))
            {
                throw new UnauthorizedAccessException(
                    "User Role Can Not Cancel Subscription");
            }

            await _SubscriptionRepo.ExecuteInTransactionAsync(
              async () =>
              {
                  var subcription = await _SubscriptionRepo.GetTable().Where(a => a.Id == subscriptionId
                  && a.Status == SubscriptionStatus.Active && a.CurrentState > 0).FirstOrDefaultAsync();

                  if (subcription == null)
                  {
                      throw new UnauthorizedAccessException(
                            "Subscription Undefined");
                  }

                  subcription.Status = SubscriptionStatus.Cancelled;
                  await _SubscriptionRepo.UpdateAsync(subcription, userId);

                  await _CancellationRecord.AddAsync(new TbCancellationRecord()
                  {
                      OrganizationId = subcription.OrganizationId,
                      SubscriptionId = subcription.Id,
                      PerformedBy = userId,
                      PerformedAt = DateTime.UtcNow,
                      EffectiveDate = DateTime.UtcNow,
                      Reason = reason,
                      Type = CancellationType.Immediate,
                      AccessEndedImmediately = true,
                      RefundInitiated = false,

                  }, userId);

                  await _AuditLogRepo.AddAsync(new TbAuditLog()
                  {
                      UserId = userId,
                      Action = "Cancel Subscription",
                      Timestamp = DateTime.UtcNow,
                      EntityType = EntityType.Subscription,
                      EntityId = subcription.Id,
                      Metadata = "Cancellation Type: Immediate",
                      CorrelationId = Guid.NewGuid()
                  }, userId);

                  await _NotificationRepo.AddAsync(new TbNotification()
                  {
                      OrganizationId = subcription.OrganizationId,
                      UserId = userId,
                      Description = "Subscription cancellation requested",
                      RelatedEntityType = EntityType.Subscription,
                      RelatedEntityId = subcription.Id,
                      IsRead = false,
                      Type = NotificationType.CancellationRequested
                  }, userId);

              },
         System.Data.IsolationLevel.Serializable);

        }



        public async Task<bool> CheckEntitlementAsync(Guid organizationId)
        {
            if (organizationId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "organizationId Is Required");
            }

            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            var currentUser =
                await _userService.GetUserByIdAsync(
                    userId.ToString());

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            var isPlatformAdmin =
                currentUser.Roles != null &&
                currentUser.Roles.Any(
                    x => x == "Platform Admin");

            var isCustomerAdmin =
                currentUser.Roles != null &&
                currentUser.Roles.Any(
                    x => x == "Customer Admin");

            var isCustomerUser =
                currentUser.Roles != null &&
                currentUser.Roles.Any(
                    x => x == "Customer User");

            if (!isPlatformAdmin &&
                !isCustomerAdmin &&
                !isCustomerUser)
            {
                throw new UnauthorizedAccessException(
                    "User Role Is Not Authorized");
            }

            if (!isPlatformAdmin &&
                currentUser.OrganizationId != organizationId)
            {
                throw new UnauthorizedAccessException(
                    "Organization Access Denied");
            }

            var organization =
                await _OrganizationRepo
                    .GetTable()
                    .Where(a =>
                        a.Id == organizationId &&
                        a.CurrentState > 0)
                    .FirstOrDefaultAsync();

            if (organization == null)
            {
                throw new UnauthorizedAccessException(
                    "Organization Undefined");
            }

            var now = DateTime.UtcNow;

            var activeTrial =
                await _TrialRepo
                    .GetTable()
                    .Where(a =>
                        a.OrganizationId == organizationId &&
                        a.CurrentState > 0 &&
                        a.Status == TrialStatus.Active &&
                        a.StartDate <= now &&
                        now < a.EndDate)
                    .FirstOrDefaultAsync();

            if (activeTrial != null)
            {
                return true;
            }

            var subscription =
                await _SubscriptionRepo
                    .GetTable()
                    .Include(a => a.BillingPeriod)
                    .Include(a => a.Invoice)
                        .ThenInclude(a => a.Payment)
                    .Include(a => a.Invoice)
                        .ThenInclude(a => a.BillingPeriod)
                    .Where(a =>
                        a.OrganizationId == organizationId &&
                        a.CurrentState > 0)
                    .OrderByDescending(a => a.CreatedDate)
                    .FirstOrDefaultAsync();

            if (subscription == null)
            {
                return false;
            }

            if (subscription.Status == SubscriptionStatus.Suspended ||
                subscription.Status == SubscriptionStatus.Expired ||
                subscription.Status == SubscriptionStatus.Cancelled ||
                subscription.Status == SubscriptionStatus.PendingPayment ||
                subscription.Status == SubscriptionStatus.PaymentFailed ||
                subscription.Status == SubscriptionStatus.PastDue)
            {
                return false;
            }

            if (subscription.Status == SubscriptionStatus.GracePeriod)
            {
                var systemConfig =
                    await _SystemConfigRepo
                        .GetTable()
                        .Where(a =>
                            a.CurrentState > 0)
                        .OrderByDescending(a => a.CreatedDate)
                        .FirstOrDefaultAsync();

                if (systemConfig == null ||
                    !systemConfig.AllowAccessDuringGracePeriod)
                {
                    return false;
                }

                var activeGracePeriod =
                    await _GracePeriodRepo
                        .GetTable()
                        .Where(a =>
                            a.SubscriptionId == subscription.Id &&
                            a.CurrentState > 0 &&
                            a.StartDate <= now &&
                            now < a.EndDate)
                        .FirstOrDefaultAsync();

                if (activeGracePeriod == null)
                {
                    return false;
                }

                return true;
            }

            if (subscription.Status != SubscriptionStatus.Active &&
                subscription.Status != SubscriptionStatus.ActiveUntilPeriodEnd)
            {
                return false;
            }

            if (!subscription.CurrentPeriodStart.HasValue ||
                !subscription.CurrentPeriodEnd.HasValue)
            {
                return false;
            }

            if (now < subscription.CurrentPeriodStart.Value ||
                now >= subscription.CurrentPeriodEnd.Value)
            {
                return false;
            }

            var currentBillingPeriod =
                subscription.BillingPeriod?
                    .Where(a =>
                        a.CurrentState > 0 &&
                        a.SubscriptionId == subscription.Id &&
                        a.Start == subscription.CurrentPeriodStart.Value &&
                        a.End == subscription.CurrentPeriodEnd.Value)
                    .FirstOrDefault();

            if (currentBillingPeriod == null)
            {
                return false;
            }

            var successfulPayment =
                subscription.Invoice?
                    .Where(invoice =>
                        invoice.CurrentState > 0 &&
                        invoice.BillingPeriodId ==
                            currentBillingPeriod.Id &&
                        invoice.BillingPeriod != null &&
                        invoice.BillingPeriod.SubscriptionId ==
                            subscription.Id &&
                        invoice.Payment != null &&
                        invoice.Payment.CurrentState > 0 &&
                        invoice.Payment.Status ==
                            PaymentStatus.Succeeded)
                    .Select(invoice => invoice.Payment)
                    .FirstOrDefault();

            if (successfulPayment == null)
            {
                return false;
            }

            return true;
        }





        public async Task CompleteSuccessfulRenewalAsync(Guid invoiceId)
        {
            if (invoiceId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "invoiceId Is Required");
            }

            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            await _SubscriptionRepo.ExecuteInTransactionAsync(
                async () =>
                {
                    var invoice =
                        await _InvoiceRepo
                            .GetTable()
                            .Include(a => a.Subscription)
                            .Include(a => a.Payment)
                            .Include(a => a.BillingPeriod)
                            .Where(a =>
                                a.Id == invoiceId &&
                                a.CurrentState > 0)
                            .FirstOrDefaultAsync();

                    if (invoice == null)
                    {
                        throw new KeyNotFoundException(
                            "Invoice Undefined");
                    }

                    if (invoice.Subscription == null)
                    {
                        throw new InvalidOperationException(
                            "Invoice Subscription Undefined");
                    }

                    if (invoice.Payment == null)
                    {
                        throw new InvalidOperationException(
                            "Payment Undefined");
                    }

                   
                    if (invoice.Status == InvoiceStatus.Paid &&
                        invoice.Payment.Status == PaymentStatus.Succeeded &&
                        invoice.BillingPeriodId.HasValue &&
                        invoice.Subscription.Status == SubscriptionStatus.Active)
                    {
                        return;
                    }

                   
                    if (invoice.Status != InvoiceStatus.Pending &&
                        invoice.Status != InvoiceStatus.PaymentFailed)
                    {
                        throw new InvalidOperationException(
                            "Invoice Is Not In A Renewable Payment State");
                    }

                    if (invoice.Payment.Status != PaymentStatus.Succeeded)
                    {
                        throw new InvalidOperationException(
                            "Payment Is Not Successful");
                    }

                    if (invoice.Payment.Amount != invoice.Amount ||
                        invoice.Payment.Currency != invoice.Currency)
                    {
                        throw new InvalidOperationException(
                            "Payment Does Not Match Invoice");
                    }

                    if (!invoice.StartDate.HasValue ||
                        !invoice.EndDate.HasValue)
                    {
                        throw new InvalidOperationException(
                            "Renewal Period Is Undefined");
                    }

                    if (invoice.EndDate.Value <= invoice.StartDate.Value)
                    {
                        throw new InvalidOperationException(
                            "Invalid Renewal Period");
                    }

                    var subscription =
                        invoice.Subscription;

                   
                    if (subscription.Status != SubscriptionStatus.Active &&
                        subscription.Status != SubscriptionStatus.PastDue &&
                        subscription.Status != SubscriptionStatus.GracePeriod)
                    {
                        throw new InvalidOperationException(
                            "Subscription Is Not In A Renewable Payment State");
                    }

                   
                    var existingBillingPeriod =
                        await _BillingPeriod
                            .GetTable()
                            .Where(a =>
                                a.SubscriptionId == invoice.SubscriptionId &&
                                a.Start == invoice.StartDate.Value &&
                                a.End == invoice.EndDate.Value &&
                                a.CurrentState > 0)
                            .FirstOrDefaultAsync();

                    TbBillingPeriod billingPeriod;

                    if (existingBillingPeriod != null)
                    {
                        if (existingBillingPeriod.PlanId != invoice.PlanId ||
                            existingBillingPeriod.Amount != invoice.Amount ||
                            existingBillingPeriod.Currency != invoice.Currency)
                        {
                            throw new InvalidOperationException(
                                "Billing Period Does Not Match Invoice");
                        }

                        billingPeriod =
                            existingBillingPeriod;
                    }
                    else
                    {
                        var overlappingBillingPeriod =
                            await _BillingPeriod
                                .GetTable()
                                .Where(a =>
                                    a.SubscriptionId == invoice.SubscriptionId &&
                                    a.CurrentState > 0 &&
                                    a.Start < invoice.EndDate.Value &&
                                    invoice.StartDate.Value < a.End)
                                .FirstOrDefaultAsync();

                        if (overlappingBillingPeriod != null)
                        {
                            throw new InvalidOperationException(
                                "Renewal Billing Period Overlaps Existing Period");
                        }

                        billingPeriod =
                            new TbBillingPeriod
                            {
                                SubscriptionId =
                                    invoice.SubscriptionId,

                                PlanId =
                                    invoice.PlanId,

                                Start =
                                    invoice.StartDate.Value,

                                End =
                                    invoice.EndDate.Value,

                                Amount =
                                    invoice.Amount,

                                Currency =
                                    invoice.Currency,

                                Status =
                                    BillingPeriodStatus.Active,

                                CurrentState = 1
                            };

                        await _BillingPeriod.AddAsync(
                            billingPeriod,
                            userId);
                    }

                   
                    invoice.BillingPeriodId =
                        billingPeriod.Id;

                    invoice.Status =
                        InvoiceStatus.Paid;

                    await _InvoiceRepo.UpdateAsync(
                        invoice,
                        userId);

                   
                    var pendingPlanChange =
                        await _PendingPlanChangeRepo
                            .GetTable()
                            .Where(a =>
                                a.SubscriptionId == subscription.Id &&
                                !a.IsApplied &&
                                a.EffectiveDate <= billingPeriod.Start &&
                                a.CurrentState > 0)
                            .OrderBy(a => a.EffectiveDate)
                            .FirstOrDefaultAsync();

                    if (pendingPlanChange != null)
                    {
                        if (pendingPlanChange.TargetPlanId !=
                            billingPeriod.PlanId)
                        {
                            throw new InvalidOperationException(
                                "Pending Plan Change Does Not Match Renewal Plan");
                        }

                        subscription.PlanId =
                            pendingPlanChange.TargetPlanId;

                        pendingPlanChange.IsApplied =
                            true;

                        await _PendingPlanChangeRepo.UpdateAsync(
                            pendingPlanChange,
                            userId);
                    }
                    else
                    {
                        subscription.PlanId =
                            billingPeriod.PlanId;
                    }

                   
                    if (subscription.Status ==
                        SubscriptionStatus.GracePeriod)
                    {
                        var activeGracePeriod =
                            await _GracePeriodRepo
                                .GetTable()
                                .Where(a =>
                                    a.SubscriptionId == subscription.Id &&
                                    a.InvoiceId == invoice.Id &&
                                    a.Status == GracePeriodStatus.Active &&
                                    a.CurrentState > 0)
                                .FirstOrDefaultAsync();

                        if (activeGracePeriod != null)
                        {
                            activeGracePeriod.Status =
                                GracePeriodStatus.Expired;

                            await _GracePeriodRepo.UpdateAsync(
                                activeGracePeriod,
                                userId);
                        }
                    }

                   
                    subscription.CurrentPeriodStart =
                        billingPeriod.Start;

                    subscription.CurrentPeriodEnd =
                        billingPeriod.End;

                    subscription.Status =
                        SubscriptionStatus.Active;

                    await _SubscriptionRepo.UpdateAsync(
                        subscription,
                        userId);

                   
                    var notification =
                        new TbNotification
                        {
                            OrganizationId =
                                subscription.OrganizationId,

                            UserId = null,

                            Description =
                                "Subscription renewal completed successfully.",

                            RelatedEntityType =
                                EntityType.Subscription,

                            RelatedEntityId =
                                subscription.Id,

                            IsRead = false,

                            Type =
                                NotificationType.RenewalSucceeded,

                            CurrentState = 1
                        };

                    await _NotificationRepo.AddAsync(
                        notification,
                        userId);

                   
                    var auditLog =
                        new TbAuditLog
                        {
                            UserId =
                                userId,

                            Action =
                                "RenewalCompleted",

                            Timestamp =
                                DateTime.UtcNow,

                            EntityType =
                                EntityType.Subscription,

                            EntityId =
                                subscription.Id,

                            Metadata =
                                $"InvoiceId: {invoice.Id}, " +
                                $"BillingPeriodId: {billingPeriod.Id}, " +
                                $"PlanId: {subscription.PlanId}",

                            CorrelationId = Guid.NewGuid(),

                            CurrentState = 1
                        };

                    await _AuditLogRepo.AddAsync(
                        auditLog,
                        userId);
                },
                System.Data.IsolationLevel.Serializable);
        }





        public async Task CreateSubscriptionAsync(SubscriptionDto dto)
        {
            if (dto.OrganizationId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "OrganizationId Is Required");
            }

            if (dto.PlanId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "PlanId Is Required");
            }

            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            var currentUser =
                await _userService.GetUserByIdAsync(userId.ToString());

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException(
                    "Current user was not found.");
            }

            if (currentUser.Role == null)
            {
                throw new UnauthorizedAccessException(
                    "Undefined User Role");
            }

            var userRoles = currentUser.Roles;

            if (!userRoles.Contains("Customer Admin"))
            {
                throw new UnauthorizedAccessException(
                    "User Can Not Create Subscription");
            }

            var organization =
                await _OrganizationRepo
                    .GetTable()
                    .Where(a =>
                        a.Id == dto.OrganizationId &&
                        a.CurrentState > 0 &&
                        a.Id == currentUser.OrganizationId)
                    .FirstOrDefaultAsync();

            if (organization == null)
            {
                throw new UnauthorizedAccessException(
                    "Organization Undefined");
            }

            var plan =
                await _PlanRepo
                    .GetTable()
                    .Where(a =>
                        a.Id == dto.PlanId &&
                        a.CurrentState > 0 &&
                        a.IsActive)
                    .FirstOrDefaultAsync();

            if (plan == null)
            {
                throw new UnauthorizedAccessException(
                    "Plan Undefined");
            }

            var invoiceId = Guid.Empty;
            var correlationId = Guid.NewGuid();

            await _SubscriptionRepo.ExecuteInTransactionAsync(
                async () =>
                {
                    var subscription = new TbSubscription
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = organization.Id,
                        PlanId = plan.Id,
                        Status = SubscriptionStatus.PendingPayment,
                        StartedAt = null,
                        CurrentPeriodStart = null,
                        CurrentPeriodEnd = null
                    };

                    await _SubscriptionRepo.AddAsync(
                        subscription,
                        userId);

                    await _AuditLogRepo.AddAsync(
                        new TbAuditLog
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            Action = "Create Subscription",
                            Timestamp = DateTime.UtcNow,
                            EntityType = EntityType.Subscription,
                            EntityId = subscription.Id,
                            Metadata = plan.Id.ToString(),
                            CorrelationId = correlationId
                        },
                        userId);
                    var startDate = DateTime.UtcNow;
                    var endDate = startDate.AddDays(plan.Duration);

                    var invoice = new TbInvoice
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = organization.Id,
                        SubscriptionId = subscription.Id,
                        PlanId = plan.Id,

                        BillingPeriodId = null,

                        Duration = plan.Duration,

                        StartDate = startDate,
                        EndDate = endDate,

                        PlanNameSnapshot = plan.PlanName,
                        PlanUserLimitSnapshot = plan.MaxUsers,

                        Amount = plan.CurrentPrice,
                        Currency = plan.Currency,

                        Status = InvoiceStatus.Pending,
                        InvoiceType = InvoiceType.Initial
                    };

                    await _InvoiceRepo.AddAsync(
                        invoice,
                        userId);

                    invoiceId = invoice.Id;
                },
                System.Data.IsolationLevel.Serializable);

            await _NotificationRepo.AddAsync(
                new TbNotification
                {
                    OrganizationId = organization.Id,
                    UserId = userId,
                    Description = "Invoice Created Successfully",
                    RelatedEntityType = EntityType.Invoice,
                    RelatedEntityId = invoiceId,
                    IsRead = false,
                    Type = NotificationType.InvoiceCreated
                },
                userId);
        }
        public async Task EnterGracePeriodAsync(Guid invoiceId)
        {
            if (invoiceId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "invoiceId Is Required");
            }

            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            await _SubscriptionRepo.ExecuteInTransactionAsync(
                async () =>
                {
                    var invoice =
                        await _InvoiceRepo
                            .GetTable()
                            .Include(a => a.Subscription)
                            .Where(a =>
                                a.Id == invoiceId &&
                                a.CurrentState > 0)
                            .FirstOrDefaultAsync();

                    if (invoice == null)
                    {
                        throw new KeyNotFoundException(
                            "Invoice Undefined");
                    }

                    if (invoice.Subscription == null)
                    {
                        throw new InvalidOperationException(
                            "Invoice Subscription Undefined");
                    }

                    var subscription = invoice.Subscription;

                   
                    var existingGracePeriod =
                        await _GracePeriodRepo
                            .GetTable()
                            .Where(a =>
                                a.InvoiceId == invoice.Id &&
                                a.SubscriptionId == subscription.Id &&
                                a.CurrentState > 0)
                            .FirstOrDefaultAsync();

                    if (existingGracePeriod != null)
                    {
                        if (subscription.Status != SubscriptionStatus.GracePeriod)
                        {
                            throw new InvalidOperationException(
                                "Grace Period Exists But Subscription Is Not In GracePeriod Status");
                        }

                        return;
                    }

                    if (invoice.Status != InvoiceStatus.PaymentFailed)
                    {
                        throw new InvalidOperationException(
                            "Invoice Is Not In PaymentFailed Status");
                    }

                    if (subscription.Status != SubscriptionStatus.PastDue)
                    {
                        throw new InvalidOperationException(
                            "Subscription Is Not In PastDue Status");
                    }

                    var config =
                        await _SystemConfigRepo
                            .GetTable()
                            .FirstOrDefaultAsync();

                    if (config == null)
                    {
                        throw new InvalidOperationException(
                            "System Billing Configuration Undefined");
                    }

                    if (config.GracePeriodDays <= 0)
                    {
                        throw new InvalidOperationException(
                            "Grace Period Duration Is Invalid");
                    }

                    var startDate = DateTime.UtcNow;

                    var endDate =
                        startDate.AddDays(config.GracePeriodDays);

                    var gracePeriod =
                        new TbGracePeriod
                        {
                            SubscriptionId = subscription.Id,
                            InvoiceId = invoice.Id,
                            StartDate = startDate,
                            EndDate = endDate,
                            Status = GracePeriodStatus.Active,
                            Reason =
                                "Renewal payment retries exhausted.",
                            CurrentState = 1
                        };

                    await _GracePeriodRepo.AddAsync(
                        gracePeriod,
                        userId);

                    subscription.Status =
                        SubscriptionStatus.GracePeriod;

                    await _SubscriptionRepo.UpdateAsync(
                        subscription,
                        userId);

                    var notification =
                        new TbNotification
                        {
                            OrganizationId =
                                subscription.OrganizationId,
                            UserId = null,
                            Description =
                                "Subscription entered grace period after renewal payment retries were exhausted.",
                            RelatedEntityType =
                                EntityType.Subscription,
                            RelatedEntityId =
                                subscription.Id,
                            IsRead = false,
                            Type =
                                NotificationType.GracePeriodStarted,
                            CurrentState = 1
                        };

                    await _NotificationRepo.AddAsync(
                        notification,
                        userId);

                    var auditLog =
                        new TbAuditLog
                        {
                            UserId = userId,
                            Action = "GracePeriodStarted",
                            Timestamp = DateTime.UtcNow,
                            EntityType =
                                EntityType.Subscription,
                            EntityId =
                                subscription.Id,
                            Metadata =
                                $"InvoiceId: {invoice.Id}, " +
                                $"GracePeriodId: {gracePeriod.Id}, " +
                                $"StartDate: {gracePeriod.StartDate:O}, " +
                                $"EndDate: {gracePeriod.EndDate:O}",
                            CorrelationId = Guid.NewGuid(),
                            CurrentState = 1
                        };

                    await _AuditLogRepo.AddAsync(
                        auditLog,
                        userId);
                },
                System.Data.IsolationLevel.Serializable);
        }

        public async Task ExpireSubscriptionAsync(Guid subscriptionId)
        {
            if (subscriptionId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "subscriptionId Is Required");
            }

            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            await _SubscriptionRepo.ExecuteInTransactionAsync(
                async () =>
                {
                    var subscription =
                        await _SubscriptionRepo
                            .GetTable()
                            .Where(a =>
                                a.Id == subscriptionId &&
                                a.CurrentState > 0)
                            .FirstOrDefaultAsync();

                    if (subscription == null)
                    {
                        throw new KeyNotFoundException(
                            "Subscription Undefined");
                    }

                   
                    if (subscription.Status == SubscriptionStatus.Expired)
                    {
                        return;
                    }

                    var previousStatus =
                        subscription.Status;

                   
                    if (subscription.Status ==
                        SubscriptionStatus.ActiveUntilPeriodEnd)
                    {
                        if (!subscription.CurrentPeriodEnd.HasValue)
                        {
                            throw new InvalidOperationException(
                                "Subscription Current Period End Is Undefined");
                        }

                        if (DateTime.UtcNow <
                            subscription.CurrentPeriodEnd.Value)
                        {
                            throw new InvalidOperationException(
                                "Subscription Paid Period Has Not Ended");
                        }
                    }
                   
                    else if (subscription.Status ==
                             SubscriptionStatus.Suspended)
                    {
                        var activeGracePeriod =
                            await _GracePeriodRepo
                                .GetTable()
                                .Where(a =>
                                    a.SubscriptionId == subscription.Id &&
                                    a.CurrentState > 0 &&
                                    a.Status == GracePeriodStatus.Active)
                                .OrderByDescending(a => a.EndDate)
                                .FirstOrDefaultAsync();

                        if (activeGracePeriod == null)
                        {
                            throw new InvalidOperationException(
                                "Active Grace Period Undefined");
                        }

                        if (DateTime.UtcNow <
                            activeGracePeriod.EndDate)
                        {
                            throw new InvalidOperationException(
                                "Grace Period Has Not Ended");
                        }

                        activeGracePeriod.Status =
                            GracePeriodStatus.Expired;

                        await _GracePeriodRepo.UpdateAsync(
                            activeGracePeriod,
                            userId);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "Subscription Cannot Be Expired From Its Current Status");
                    }

                    subscription.Status =
                        SubscriptionStatus.Expired;

                    await _SubscriptionRepo.UpdateAsync(
                        subscription,
                        userId);

                    var notification =
                        new TbNotification
                        {
                            OrganizationId =
                                subscription.OrganizationId,
                            UserId = null,
                            Description =
                                "Subscription has expired.",
                            RelatedEntityType =
                                EntityType.Subscription,
                            RelatedEntityId =
                                subscription.Id,
                            IsRead = false,
                            Type =
                                NotificationType.SubscriptionExpired,
                            CurrentState = 1
                        };

                    await _NotificationRepo.AddAsync(
                        notification,
                        userId);

                    var auditLog =
                        new TbAuditLog
                        {
                            UserId = userId,
                            Action = "SubscriptionExpired",
                            Timestamp = DateTime.UtcNow,
                            EntityType =
                                EntityType.Subscription,
                            EntityId =
                                subscription.Id,
                            Metadata =
                                $"SubscriptionId: {subscription.Id}, " +
                                $"OrganizationId: {subscription.OrganizationId}, " +
                                $"PreviousStatus: {previousStatus}, " +
                                $"NewStatus: {SubscriptionStatus.Expired}, " +
                                $"ExpiredAt: {DateTime.UtcNow:O}",
                            CorrelationId = Guid.NewGuid(),
                            CurrentState = 1
                        };

                    await _AuditLogRepo.AddAsync(
                        auditLog,
                        userId);
                },
                System.Data.IsolationLevel.Serializable);
        }


        public async Task<SubscriptionDto> GetSubscriptionByIdAsync(Guid subscriptionId)
        {
            if (subscriptionId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "subscriptionId Is Required");
            }

            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            var currentUser = await _userService.GetUserByIdAsync(userId.ToString());

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException(
                    "Current user was not found.");
            }

            var userRoles = currentUser.Roles;

            if (userRoles.Contains("Platform Admin"))
            {
                var subscription = await _SubscriptionRepo.GetByIdAsync(subscriptionId);

                if (subscription == null)
                {
                    throw new UnauthorizedAccessException(
                        "Subscription Undefined");
                }

                return _mapper.Map<TbSubscription, SubscriptionDto>(subscription);
            }

            if (userRoles.Contains("Customer Admin"))
            {
                var subscription = await _SubscriptionRepo.GetTable()
                    .Where(a =>
                        a.Id == subscriptionId &&
                        a.OrganizationId == currentUser.OrganizationId)
                    .FirstOrDefaultAsync();

                if (subscription == null)
                {
                    throw new UnauthorizedAccessException(
                        "Subscription Undefined");
                }

                return _mapper.Map<TbSubscription, SubscriptionDto>(subscription);
            }

            throw new UnauthorizedAccessException(
                "User Role Can Not Get SubscriptionById");
        }


        public async Task<SubscriptionDto> GetSubscriptionByOrganizationIdAsync(Guid organizationId)
        {
            if (organizationId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                   "organizationId Is Required");
            }

            var userId = _userService.GetLoggedInUser();
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            var currentUser = await _userService.GetUserByIdAsync(userId.ToString());

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException(
                    "Current user was not found.");
            }

            var userRole = currentUser.Roles;

            if (userRole.Contains("Platform Admin"))
            {
                var subcription = await _SubscriptionRepo.GetTable().Where(a => a.OrganizationId == organizationId).FirstOrDefaultAsync();
                if (subcription == null)
                {

                    throw new UnauthorizedAccessException(
                        "Subscription Undefined");
                }

                return _mapper.Map<TbSubscription, SubscriptionDto>(subcription);

            }

            if (userRole.Contains("Customer Admin"))
            {
                var subcription = await _SubscriptionRepo.GetTable()
                    .Where(a => a.OrganizationId == organizationId &&
                    a.OrganizationId == currentUser.OrganizationId)
                    .FirstOrDefaultAsync();

                if (subcription == null)
                {
                    throw new UnauthorizedAccessException(
                      "Subscription Undefined");
                }

                return _mapper.Map<TbSubscription, SubscriptionDto>(subcription);
            }


            throw new UnauthorizedAccessException(
                "User Role Can Not Get SubscriptionById");

        }

        public async Task HandleInitialPaymentFailureAsync(Guid invoiceId)
        {
            if (invoiceId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "invoiceId Is Required");
            }

            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            await _SubscriptionRepo.ExecuteInTransactionAsync(
                async () =>
                {
                    var invoice =
                        await _InvoiceRepo
                            .GetTable()
                            .Include(a => a.Subscription)
                            .Include(a => a.Payment)
                            .Where(a =>
                                a.Id == invoiceId &&
                                a.CurrentState > 0)
                            .FirstOrDefaultAsync();

                    if (invoice == null)
                    {
                        throw new KeyNotFoundException(
                            "Invoice Undefined");
                    }

                    if (invoice.Subscription == null)
                    {
                        throw new InvalidOperationException(
                            "Invoice Subscription Undefined");
                    }

                    var subscription =
                        invoice.Subscription;

                   
                    if (invoice.Status == InvoiceStatus.PaymentFailed &&
                        subscription.Status == SubscriptionStatus.PaymentFailed)
                    {
                        return;
                    }

                  
                    if (invoice.Payment != null &&
                        invoice.Payment.Status == PaymentStatus.Succeeded)
                    {
                        throw new InvalidOperationException(
                            "Invoice Already Has A Successful Payment");
                    }

                  
                    if (invoice.Status != InvoiceStatus.Pending)
                    {
                        throw new InvalidOperationException(
                            "Invoice Is Not Pending");
                    }

                    if (subscription.Status !=
                        SubscriptionStatus.PendingPayment)
                    {
                        throw new InvalidOperationException(
                            "Subscription Is Not Pending Payment");
                    }

                   
                    invoice.Status =
                        InvoiceStatus.PaymentFailed;

                    await _InvoiceRepo.UpdateAsync(
                        invoice,
                        userId);

                    subscription.Status =
                        SubscriptionStatus.PaymentFailed;

                    await _SubscriptionRepo.UpdateAsync(
                        subscription,
                        userId);

                    
                    var notification =
                        new TbNotification
                        {
                            OrganizationId =
                                subscription.OrganizationId,
                            UserId = null,
                            Description =
                                "Initial subscription payment failed.",
                            RelatedEntityType =
                                EntityType.Subscription,
                            RelatedEntityId =
                                subscription.Id,
                            IsRead = false,
                            Type =
                                NotificationType.PaymentFailed,
                            CurrentState = 1
                        };

                    await _NotificationRepo.AddAsync(
                        notification,
                        userId);

                   
                    var auditLog =
                        new TbAuditLog
                        {
                            UserId = userId,
                            Action = "InitialPaymentFailed",
                            Timestamp = DateTime.UtcNow,
                            EntityType =
                                EntityType.Subscription,
                            EntityId =
                                subscription.Id,
                            Metadata =
                                $"InvoiceId: {invoice.Id}, " +
                                $"SubscriptionId: {subscription.Id}, " +
                                $"OrganizationId: {subscription.OrganizationId}, " +
                                $"InvoiceStatus: {InvoiceStatus.PaymentFailed}, " +
                                $"SubscriptionStatus: {SubscriptionStatus.PaymentFailed}",
                            CorrelationId = Guid.NewGuid(),
                            CurrentState = 1
                        };

                    await _AuditLogRepo.AddAsync(
                        auditLog,
                        userId);
                },
                System.Data.IsolationLevel.Serializable);
        }


public async Task HandleRenewalFailureAsync(Guid invoiceId)
        {
            if (invoiceId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "invoiceId Is Required");
            }

            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            await _SubscriptionRepo.ExecuteInTransactionAsync(
                async () =>
                {
                    var invoice =
                        await _InvoiceRepo
                            .GetTable()
                            .Include(a => a.Subscription)
                            .Include(a => a.Payment)
                            .Include(a => a.BillingPeriod)
                            .Include(a => a.PaymentAttempt)
                            .Where(a =>
                                a.Id == invoiceId &&
                                a.CurrentState > 0)
                            .FirstOrDefaultAsync();

                    if (invoice == null)
                    {
                        throw new KeyNotFoundException(
                            "Invoice Undefined");
                    }

                    if (invoice.Subscription == null)
                    {
                        throw new InvalidOperationException(
                            "Invoice Subscription Undefined");
                    }

                    if (!invoice.BillingPeriodId.HasValue ||
                        invoice.BillingPeriod == null)
                    {
                        throw new InvalidOperationException(
                            "Renewal Billing Period Undefined");
                    }

                    var subscription =
                        invoice.Subscription;

                    if (invoice.Payment != null &&
                        invoice.Payment.Status == PaymentStatus.Succeeded)
                    {
                        throw new InvalidOperationException(
                            "Invoice Already Has A Successful Payment");
                    }

                    var existingGracePeriod =
                        await _GracePeriodRepo
                            .GetTable()
                            .Where(a =>
                                a.InvoiceId == invoice.Id &&
                                a.SubscriptionId == subscription.Id &&
                                a.CurrentState > 0)
                            .FirstOrDefaultAsync();

                    if (existingGracePeriod != null)
                    {
                        return;
                    }

                    if (invoice.Status != InvoiceStatus.Pending &&
                        invoice.Status != InvoiceStatus.PaymentFailed)
                    {
                        throw new InvalidOperationException(
                            "Invoice Is Not In A Renewable Failure State");
                    }

                    if (subscription.Status != SubscriptionStatus.Active &&
                        subscription.Status != SubscriptionStatus.PastDue)
                    {
                        throw new InvalidOperationException(
                            "Subscription Is Not In A Renewable Failure State");
                    }

                    if (invoice.SubscriptionId !=
                        invoice.BillingPeriod.SubscriptionId)
                    {
                        throw new InvalidOperationException(
                            "Renewal Billing Period Does Not Belong To Subscription");
                    }

                    if (invoice.PlanId !=
                        invoice.BillingPeriod.PlanId)
                    {
                        throw new InvalidOperationException(
                            "Renewal Plan Does Not Match Billing Period");
                    }

                    if (invoice.Amount !=
                            invoice.BillingPeriod.Amount ||
                        invoice.Currency !=
                            invoice.BillingPeriod.Currency)
                    {
                        throw new InvalidOperationException(
                            "Renewal Amount Does Not Match Billing Period");
                    }

                    var config =
                        await _SystemConfigRepo
                            .GetTable()
                            .FirstOrDefaultAsync();

                    if (config == null)
                    {
                        throw new InvalidOperationException(
                            "System Billing Configuration Undefined");
                    }

                    if (config.RetryDay1 < 0 ||
                        config.RetryDay2 < 0 ||
                        config.RetryDay3 < 0)
                    {
                        throw new InvalidOperationException(
                            "Renewal Retry Configuration Is Invalid");
                    }

                    if (config.GracePeriodDays <= 0)
                    {
                        throw new InvalidOperationException(
                            "Grace Period Duration Is Invalid");
                    }

                   
                    var retryDays = new[]
                    {
                config.RetryDay1,
                config.RetryDay2,
                config.RetryDay3
                    };

                    var maxAttempts =
                        1 + retryDays.Length;

                    var paymentAttemptCount =
                        invoice.PaymentAttempt?
                            .Count(a => a.CurrentState > 0)
                        ?? 0;

                    if (paymentAttemptCount <= 0)
                    {
                        throw new InvalidOperationException(
                            "Renewal Payment Attempt Undefined");
                    }

                    invoice.Status =
                        InvoiceStatus.PaymentFailed;

                    await _InvoiceRepo.UpdateAsync(
                        invoice,
                        userId);

                    if (paymentAttemptCount < maxAttempts)
                    {
                        subscription.Status =
                            SubscriptionStatus.PastDue;

                        await _SubscriptionRepo.UpdateAsync(
                            subscription,
                            userId);

                        var notification =
                            new TbNotification
                            {
                                OrganizationId =
                                    subscription.OrganizationId,

                                UserId = null,

                                Description =
                                    "Renewal payment failed.",

                                RelatedEntityType =
                                    EntityType.Subscription,

                                RelatedEntityId =
                                    subscription.Id,

                                IsRead = false,

                                Type =
                                    NotificationType.PaymentFailed,

                                CurrentState = 1
                            };

                        await _NotificationRepo.AddAsync(
                            notification,
                            userId);

                        var auditLog =
                            new TbAuditLog
                            {
                                UserId =
                                    userId,

                                Action =
                                    "RenewalPaymentFailed",

                                Timestamp =
                                    DateTime.UtcNow,

                                EntityType =
                                    EntityType.Subscription,

                                EntityId =
                                    subscription.Id,

                                Metadata =
                                    $"InvoiceId: {invoice.Id}, " +
                                    $"SubscriptionId: {subscription.Id}, " +
                                    $"OrganizationId: {subscription.OrganizationId}, " +
                                    $"PaymentAttemptCount: {paymentAttemptCount}, " +
                                    $"InvoiceStatus: {InvoiceStatus.PaymentFailed}, " +
                                    $"SubscriptionStatus: {SubscriptionStatus.PastDue}",

                                CorrelationId =
                                    Guid.NewGuid(),

                                CurrentState = 1
                            };

                        await _AuditLogRepo.AddAsync(
                            auditLog,
                            userId);

                        return;
                    }

                    var startDate =
                        DateTime.UtcNow;

                    var endDate =
                        startDate.AddDays(
                            config.GracePeriodDays);

                    var gracePeriod =
                        new TbGracePeriod
                        {
                            SubscriptionId =
                                subscription.Id,

                            InvoiceId =
                                invoice.Id,

                            StartDate =
                                startDate,

                            EndDate =
                                endDate,

                            Status =
                                GracePeriodStatus.Active,

                            Reason =
                                "Renewal payment retries exhausted.",

                            CurrentState = 1
                        };

                    await _GracePeriodRepo.AddAsync(
                        gracePeriod,
                        userId);

                    subscription.Status =
                        SubscriptionStatus.GracePeriod;

                    await _SubscriptionRepo.UpdateAsync(
                        subscription,
                        userId);

                    var graceNotification =
                        new TbNotification
                        {
                            OrganizationId =
                                subscription.OrganizationId,

                            UserId = null,

                            Description =
                                "Subscription entered grace period after renewal payment retries were exhausted.",

                            RelatedEntityType =
                                EntityType.Subscription,

                            RelatedEntityId =
                                subscription.Id,

                            IsRead = false,

                            Type =
                                NotificationType.GracePeriodStarted,

                            CurrentState = 1
                        };

                    await _NotificationRepo.AddAsync(
                        graceNotification,
                        userId);

                    var graceAuditLog =
                        new TbAuditLog
                        {
                            UserId =
                                userId,

                            Action =
                                "GracePeriodStarted",

                            Timestamp =
                                DateTime.UtcNow,

                            EntityType =
                                EntityType.Subscription,

                            EntityId =
                                subscription.Id,

                            Metadata =
                                $"InvoiceId: {invoice.Id}, " +
                                $"GracePeriodId: {gracePeriod.Id}, " +
                                $"PaymentAttemptCount: {paymentAttemptCount}, " +
                                $"StartDate: {gracePeriod.StartDate:O}, " +
                                $"EndDate: {gracePeriod.EndDate:O}",

                            CorrelationId =
                                Guid.NewGuid(),

                            CurrentState = 1
                        };

                    await _AuditLogRepo.AddAsync(
                        graceAuditLog,
                        userId);
                },
                System.Data.IsolationLevel.Serializable);
        }

        public async Task RenewSubscriptionAsync(Guid subscriptionId)
        {
            if (subscriptionId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "subscriptionId Is Required");
            }

            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            await _SubscriptionRepo.ExecuteInTransactionAsync(
                async () =>
                {
                    var subscription =
                        await _SubscriptionRepo
                            .GetTable()
                            .Include(a => a.Plan)
                            .Where(a =>
                                a.Id == subscriptionId &&
                                a.CurrentState > 0)
                            .FirstOrDefaultAsync();

                    if (subscription == null)
                    {
                        throw new KeyNotFoundException(
                            "Subscription Undefined");
                    }

                    if (subscription.Plan == null)
                    {
                        throw new InvalidOperationException(
                            "Subscription Plan Undefined");
                    }

                    if (subscription.Status != SubscriptionStatus.Active)
                    {
                        throw new InvalidOperationException(
                            "Subscription Is Not Active");
                    }

                    if (!subscription.CurrentPeriodEnd.HasValue)
                    {
                        throw new InvalidOperationException(
                            "Current Billing Period End Is Undefined");
                    }

                    var startDate =
                        subscription.CurrentPeriodEnd.Value;

                   
                    var pendingPlanChange =
                        await _PendingPlanChangeRepo
                            .GetTable()
                            .Where(a =>
                                a.SubscriptionId == subscription.Id &&
                                !a.IsApplied &&
                                a.EffectiveDate <= startDate &&
                                a.CurrentState > 0)
                            .OrderBy(a => a.EffectiveDate)
                            .FirstOrDefaultAsync();

                    TbPlan targetPlan;

                    if (pendingPlanChange != null)
                    {
                        targetPlan =
                            await _PlanRepo
                                .GetTable()
                                .Where(a =>
                                    a.Id == pendingPlanChange.TargetPlanId &&
                                    a.CurrentState > 0)
                                .FirstOrDefaultAsync();

                        if (targetPlan == null)
                        {
                            throw new KeyNotFoundException(
                                "Pending Target Plan Undefined");
                        }
                    }
                    else
                    {
                        targetPlan =
                            subscription.Plan;
                    }

                    if (targetPlan.Duration <= 0)
                    {
                        throw new InvalidOperationException(
                            "Plan Duration Is Invalid");
                    }

                    var endDate =
                        startDate.AddDays(targetPlan.Duration);

                   
                    var existingInvoice =
                        await _InvoiceRepo
                            .GetTable()
                            .Where(a =>
                                a.SubscriptionId == subscription.Id &&
                                a.InvoiceType == InvoiceType.Renewal &&
                                a.StartDate == startDate &&
                                a.EndDate == endDate &&
                                a.CurrentState > 0)
                            .FirstOrDefaultAsync();

                    if (existingInvoice != null)
                    {
                        return;
                    }

                  
                    var existingBillingPeriod =
                        await _BillingPeriod
                            .GetTable()
                            .Where(a =>
                                a.SubscriptionId == subscription.Id &&
                                a.Start == startDate &&
                                a.End == endDate &&
                                a.CurrentState > 0)
                            .FirstOrDefaultAsync();

                    if (existingBillingPeriod != null)
                    {
                        throw new InvalidOperationException(
                            "Renewal Billing Period Already Exists");
                    }

                    var overlappingBillingPeriod =
                        await _BillingPeriod
                            .GetTable()
                            .Where(a =>
                                a.SubscriptionId == subscription.Id &&
                                a.CurrentState > 0 &&
                                a.Start < endDate &&
                                startDate < a.End)
                            .FirstOrDefaultAsync();

                    if (overlappingBillingPeriod != null)
                    {
                        throw new InvalidOperationException(
                            "Renewal Billing Period Overlaps Existing Period");
                    }

                   
                    var invoice =
                        new TbInvoice
                        {
                            OrganizationId =
                                subscription.OrganizationId,

                            SubscriptionId =
                                subscription.Id,

                            PlanId =
                                targetPlan.Id,

                            BillingPeriodId =
                                null,

                            Duration =
                                targetPlan.Duration,

                            StartDate =
                                startDate,

                            EndDate =
                                endDate,

                            PlanNameSnapshot =
                                targetPlan.PlanName,

                            PlanUserLimitSnapshot =
                                targetPlan.MaxUsers,

                            Amount =
                                targetPlan.CurrentPrice,

                            Currency =
                                targetPlan.Currency,

                            Status =
                                InvoiceStatus.Pending,

                            InvoiceType =
                                InvoiceType.Renewal,

                            CurrentState = 1
                        };

                    await _InvoiceRepo.AddAsync(
                        invoice,
                        userId);

                    var notification =
                        new TbNotification
                        {
                            OrganizationId =
                                subscription.OrganizationId,

                            UserId = null,

                            Description =
                                "Renewal invoice created.",

                            RelatedEntityType =
                                EntityType.Invoice,

                            RelatedEntityId =
                                invoice.Id,

                            IsRead = false,

                            Type =
                                NotificationType.InvoiceCreated,

                            CurrentState = 1
                        };

                    await _NotificationRepo.AddAsync(
                        notification,
                        userId);

                    var auditLog =
                        new TbAuditLog
                        {
                            UserId =
                                userId,

                            Action =
                                "RenewalInvoiceCreated",

                            Timestamp =
                                DateTime.UtcNow,

                            EntityType =
                                EntityType.Subscription,

                            EntityId =
                                subscription.Id,

                            Metadata =
                                $"InvoiceId: {invoice.Id}, " +
                                $"SubscriptionId: {subscription.Id}, " +
                                $"PlanId: {targetPlan.Id}, " +
                                $"Amount: {invoice.Amount}, " +
                                $"Currency: {invoice.Currency}, " +
                                $"StartDate: {invoice.StartDate:O}, " +
                                $"EndDate: {invoice.EndDate:O}",

                            CorrelationId = Guid.NewGuid(),

                            CurrentState = 1
                        };

                    await _AuditLogRepo.AddAsync(
                        auditLog,
                        userId);
                },
                System.Data.IsolationLevel.Serializable);
        }

        public async Task RequestDowngradeAsync(
          Guid subscriptionId,
             Guid targetPlanId)
              {
            if (subscriptionId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "subscriptionId Is Required");
            }

            if (targetPlanId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "targetPlanId Is Required");
            }

            var userId =
                _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            var currentUser =
                await _userService.GetUserByIdAsync(
                    userId.ToString());

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            if (!currentUser.Roles.Contains("Customer Admin"))
            {
                throw new UnauthorizedAccessException(
                    "User Is Not Authorized To Request Downgrade");
            }

            if (!currentUser.OrganizationId.HasValue)
            {
                throw new UnauthorizedAccessException(
                    "User Organization Undefined");
            }

            await _SubscriptionRepo.ExecuteInTransactionAsync(
                async () =>
                {
                    var subscription =
                        await _SubscriptionRepo
                            .GetTable()
                            .Include(a => a.Plan)
                            .Where(a =>
                                a.Id == subscriptionId &&
                                a.OrganizationId ==
                                    currentUser.OrganizationId.Value &&
                                a.CurrentState > 0)
                            .FirstOrDefaultAsync();

                    if (subscription == null)
                    {
                        throw new KeyNotFoundException(
                            "Subscription Undefined");
                    }

                    if (subscription.Plan == null)
                    {
                        throw new InvalidOperationException(
                            "Subscription Plan Undefined");
                    }

                    if (subscription.Status !=
                        SubscriptionStatus.Active)
                    {
                        throw new InvalidOperationException(
                            "Subscription Is Not Active");
                    }

                    if (!subscription.CurrentPeriodEnd.HasValue)
                    {
                        throw new InvalidOperationException(
                            "Current Billing Period End Is Undefined");
                    }

                    var targetPlan =
                        await _PlanRepo
                            .GetTable()
                            .Where(a =>
                                a.Id == targetPlanId &&
                                a.CurrentState > 0)
                            .FirstOrDefaultAsync();

                    if (targetPlan == null)
                    {
                        throw new KeyNotFoundException(
                            "Target Plan Undefined");
                    }

                    if (targetPlan.Id ==
                        subscription.PlanId)
                    {
                        throw new InvalidOperationException(
                            "Target Plan Is The Current Plan");
                    }

                    var pendingPlanChange =
                        await _PendingPlanChangeRepo
                            .GetTable()
                            .Where(a =>
                                a.SubscriptionId ==
                                    subscription.Id &&
                                !a.IsApplied &&
                                a.CurrentState > 0)
                            .FirstOrDefaultAsync();

                    if (pendingPlanChange != null)
                    {
                        throw new InvalidOperationException(
                            "A Pending Plan Change Already Exists");
                    }

                    var pendingChange =
                        new TbPendingPlanChange
                        {
                            SubscriptionId =
                                subscription.Id,

                            TargetPlanId =
                                targetPlan.Id,

                            EffectiveDate =
                                subscription.CurrentPeriodEnd.Value,

                            IsApplied = false,

                            CurrentState = 1
                        };

                    await _PendingPlanChangeRepo.AddAsync(
                        pendingChange,
                        userId);

                    var notification =
                        new TbNotification
                        {
                            OrganizationId =
                                subscription.OrganizationId,

                            UserId = null,

                            Description =
                                "Subscription downgrade requested.",

                            RelatedEntityType =
                                EntityType.Subscription,

                            RelatedEntityId =
                                subscription.Id,

                            IsRead = false,

                            Type =
                                NotificationType.PlanChanged,

                            CurrentState = 1
                        };

                    await _NotificationRepo.AddAsync(
                        notification,
                        userId);

                    var auditLog =
                        new TbAuditLog
                        {
                            UserId =
                                userId,

                            Action =
                                "SubscriptionDowngradeRequested",

                            Timestamp =
                                DateTime.UtcNow,

                            EntityType =
                                EntityType.Subscription,

                            EntityId =
                                subscription.Id,

                            Metadata =
                                $"CurrentPlanId: {subscription.PlanId}, " +
                                $"TargetPlanId: {targetPlan.Id}, " +
                                $"EffectiveDate: {pendingChange.EffectiveDate:O}",

                            CorrelationId = Guid.NewGuid(),

                            CurrentState = 1
                        };

                    await _AuditLogRepo.AddAsync(
                        auditLog,
                        userId);
                },
                System.Data.IsolationLevel.Serializable);
        }


        public async Task SuspendSubscriptionAsync(
     Guid subscriptionId)
        {
            if (subscriptionId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "subscriptionId Is Required");
            }

            var userId =
                _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            await _SubscriptionRepo.ExecuteInTransactionAsync(
                async () =>
                {
                    var subscription =
                        await _SubscriptionRepo
                            .GetTable()
                            .Where(a =>
                                a.Id == subscriptionId &&
                                a.CurrentState > 0)
                            .FirstOrDefaultAsync();

                    if (subscription == null)
                    {
                        throw new KeyNotFoundException(
                            "Subscription Undefined");
                    }

                    if (subscription.Status !=
                        SubscriptionStatus.GracePeriod)
                    {
                        throw new InvalidOperationException(
                            "Subscription Is Not In Grace Period");
                    }

                    var gracePeriod =
                        await _GracePeriodRepo
                            .GetTable()
                            .Where(a =>
                                a.SubscriptionId ==
                                    subscription.Id &&
                                a.Status ==
                                    GracePeriodStatus.Active &&
                                a.CurrentState > 0)
                            .OrderByDescending(a => a.EndDate)
                            .FirstOrDefaultAsync();

                    if (gracePeriod == null)
                    {
                        throw new InvalidOperationException(
                            "Active Grace Period Undefined");
                    }

                    if (DateTime.UtcNow <
                        gracePeriod.EndDate)
                    {
                        throw new InvalidOperationException(
                            "Grace Period Has Not Ended");
                    }

                    subscription.Status =
                        SubscriptionStatus.Suspended;

                    await _SubscriptionRepo.UpdateAsync(
                        subscription,
                        userId);

                    var notification =
                        new TbNotification
                        {
                            OrganizationId =
                                subscription.OrganizationId,

                            UserId = null,

                            Description =
                                "Subscription suspended.",

                            RelatedEntityType =
                                EntityType.Subscription,

                            RelatedEntityId =
                                subscription.Id,

                            IsRead = false,

                            Type =
                                NotificationType.SubscriptionSuspended,

                            CurrentState = 1
                        };

                    await _NotificationRepo.AddAsync(
                        notification,
                        userId);

                    var auditLog =
                        new TbAuditLog
                        {
                            UserId =
                                userId,

                            Action =
                                "SubscriptionSuspended",

                            Timestamp =
                                DateTime.UtcNow,

                            EntityType =
                                EntityType.Subscription,

                            EntityId =
                                subscription.Id,

                            Metadata =
                                $"GracePeriodId: {gracePeriod.Id}, " +
                                $"GracePeriodEndDate: {gracePeriod.EndDate:O}",

                            CorrelationId = Guid.NewGuid(),

                            CurrentState = 1
                        };

                    await _AuditLogRepo.AddAsync(
                        auditLog,
                        userId);
                },
                System.Data.IsolationLevel.Serializable);
        }

        public async Task UpgradeSubscriptionAsync(Guid subscriptionId, Guid targetPlanId)
        {
            if (subscriptionId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                "subscriptionId Is Required");
            }

            if (targetPlanId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
               "targetPlanId Is Required");
            }

            var userId = _userService.GetLoggedInUser();
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User Undefined");
            }

            var currentUser = await _userService.GetUserByIdAsync(userId.ToString());
            if (currentUser == null)
            {
                throw new UnauthorizedAccessException(
                   "Current user was not found.");
            }

            var userRoles = currentUser.Roles;



            if (!userRoles.Contains("Customer Admin"))
            {
                throw new UnauthorizedAccessException(
                   "User Role Can Not Upgrad Subcription");
            }


            var subcription = await _SubscriptionRepo.GetTable().Where(a => a.Id == subscriptionId && a.OrganizationId == currentUser.OrganizationId && a.Status
            == SubscriptionStatus.Active && a.CurrentState > 0).FirstOrDefaultAsync();
            if (subcription == null)
            {
                throw new UnauthorizedAccessException(
                         "Subscription Undefined");
            }

            var currentPlan = await _PlanRepo.GetTable().Where(a => a.Id == subcription.PlanId && a.IsActive && a.CurrentState > 0).FirstOrDefaultAsync();
            if (currentPlan == null)
            {
                throw new UnauthorizedAccessException(
                     "plan Undefined");
            }

            var targetPlan = await _PlanRepo.GetTable().Where(a => a.Id == targetPlanId && a.IsActive && a.CurrentState > 0).FirstOrDefaultAsync();
            if (targetPlan == null)
            {
                throw new UnauthorizedAccessException(
                        "target plan Undefined");
            }

            if (currentPlan.PlanName == targetPlan.PlanName || targetPlan.CurrentPrice <= currentPlan.CurrentPrice)
            {
                throw new UnauthorizedAccessException(
                       "Can Not By Upgrated");
            }

            if (!subcription.CurrentPeriodStart.HasValue ||
            !subcription.CurrentPeriodEnd.HasValue)
            {
                throw new UnauthorizedAccessException(
                    "Current billing period dates are required.");
            }


            var remainingPeriod =
            subcription.CurrentPeriodEnd.Value - DateTime.UtcNow;

            var remainingDays = remainingPeriod.TotalDays;

            var priceCurrentPlan = (currentPlan.CurrentPrice * (decimal)remainingDays) / currentPlan.Duration;
            var pricetargetPlan = (targetPlan.CurrentPrice * (decimal)remainingDays) / targetPlan.Duration;
            var duePrice = Math.Round(pricetargetPlan - priceCurrentPlan, 2);

            var getBillingPeriod = await _BillingPeriod.GetTable().Where(a => a.SubscriptionId ==
               subcription.Id && a.Status == BillingPeriodStatus.Active && a.CurrentState > 0).FirstOrDefaultAsync();
            if (getBillingPeriod == null)
            {
                throw new UnauthorizedAccessException(
                     "BillingPeriod Undefined");
            }
            await _SubscriptionRepo.ExecuteInTransactionAsync(
            async () =>
            {
                var exists = await _InvoiceRepo.GetTable()
                 .AnyAsync(a =>
                     a.SubscriptionId == subcription.Id &&
                     a.PlanId == targetPlan.Id &&
                     a.BillingPeriodId == getBillingPeriod.Id &&
                     a.InvoiceType == InvoiceType.Proration &&
                     a.Status == InvoiceStatus.Pending &&
                     a.CurrentState > 0);

                if (exists)
                {
                    throw new UnauthorizedAccessException(
                      "A proration invoice already exists for this upgrade.");
                }

                var invoice = new TbInvoice()
                {
                    OrganizationId = subcription.OrganizationId,
                    SubscriptionId = subcription.Id,
                    PlanId = targetPlan.Id,
                    BillingPeriodId = getBillingPeriod.Id,
                    Duration = targetPlan.Duration,
                    StartDate = subcription.CurrentPeriodStart.Value,
                    EndDate = subcription.CurrentPeriodEnd.Value,
                    PlanNameSnapshot = targetPlan.PlanName,
                    PlanUserLimitSnapshot = targetPlan.MaxUsers,
                    Amount = duePrice,
                    Currency = targetPlan.Currency,
                    Status = InvoiceStatus.Pending,
                    InvoiceType = InvoiceType.Proration
                };


                await _InvoiceRepo.AddAsync(invoice, userId);



                await _NotificationRepo.AddAsync(new TbNotification()
                {
                    OrganizationId = subcription.OrganizationId,
                    UserId = userId,
                    Description = "Proration invoice created.",
                    RelatedEntityType = EntityType.Invoice,
                    RelatedEntityId = invoice.Id,
                    IsRead = false,
                    Type = NotificationType.InvoiceCreated
                }, userId);

                await _AuditLogRepo.AddAsync(new TbAuditLog()
                {
                    UserId = userId,
                    Action = "Upgrade Subscription",
                    Timestamp = DateTime.UtcNow,
                    EntityType = EntityType.Subscription,
                    EntityId = subcription.Id,
                    Metadata = $"Upgrade from Plan {currentPlan.Id} to Plan {targetPlan.Id}",
                    CorrelationId = Guid.NewGuid()
                }, userId);

            },
               System.Data.IsolationLevel.Serializable);




        }
    }
}