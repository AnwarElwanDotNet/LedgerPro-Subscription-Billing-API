using AutoMapper;
using BL.Contract;
using BL.Dtos;
using DAL.Contracts;
using DAL.DBContext;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BL.Exceptions;
using System.Text.Json;
namespace BL.Services
{
    public class PaymentService : BaseService<TbPayment, PaymentDto>, IPayment
    {
        private readonly ITableRepository<TbPayment> _PaymentRepo;
        private readonly ITableRepository<TbPaymentAttempt> _PaymentAttemptRepo;
        private readonly ITableRepository<TbInvoice> _InvoiceRepo;
        private readonly ITableRepository<TbOrganization> _OrganizationRepo;
        private readonly ITableRepository<TbSubscription> _SubscriptionRepo;
        private readonly INotification _notificationService;
        private readonly IAuditLog _auditLogService;
        private readonly IPaymentAttempt _paymentAttemptService;

        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly LedgerProDBContext _context;

        public PaymentService(
            ITableRepository<TbPayment> tableRepository,
            ITableRepository<TbPaymentAttempt> paymentAttemptRepository,
            ITableRepository<TbInvoice> invoiceRepository,
            ITableRepository<TbOrganization> organizationRepository,
            ITableRepository<TbSubscription> subscriptionRepository,
            IPaymentAttempt paymentAttemptService,
            IMapper mapper,
            IUserService userService,
            LedgerProDBContext context, INotification notificationService,
            IAuditLog auditLogService)
            : base(tableRepository, mapper, userService)
        {
            _PaymentRepo = tableRepository;
            _PaymentAttemptRepo = paymentAttemptRepository;
            _InvoiceRepo = invoiceRepository;
            _OrganizationRepo = organizationRepository;
            _SubscriptionRepo = subscriptionRepository;

            _paymentAttemptService = paymentAttemptService;
            _notificationService = notificationService;
            _auditLogService = auditLogService;
            _mapper = mapper;
            _userService = userService;
            _context = context;
        }

        // =========================================================
        // Get Payment By Id
        // =========================================================
        public override async Task<PaymentDto?> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(
                    "Payment ID is required.",
                    nameof(id));
            var user = await GetCurrentUserAsync();

            EnsurePaymentReadAccess(user);

            var payment = await _PaymentRepo
                .GetTable()
                .Include(x => x.Invoice)
                    .ThenInclude(x => x.Plan)
               .FirstOrDefaultAsync(
    x => x.Id == id &&
         x.CurrentState > 0);

            if (payment == null)
                return null;

            EnsureOrganizationAccess(
                user,
                payment.Invoice?.OrganizationId);

            return _mapper.Map<PaymentDto>(payment);
        }

        // =========================================================
        // Get Payments By Invoice
        // =========================================================
        public async Task<List<PaymentDto>> GetByInvoiceIdAsync(
         Guid invoiceId)
        {
            if (invoiceId == Guid.Empty)
                throw new ArgumentException(
                    "Invoice ID is required.",
                    nameof(invoiceId));

            var user = await GetCurrentUserAsync();

            EnsurePaymentReadAccess(user);

            var invoice = await _InvoiceRepo
                .GetTable()
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == invoiceId &&
                        x.CurrentState > 0);

            if (invoice == null)
                throw new KeyNotFoundException(
                    "Invoice not found.");

            EnsureOrganizationAccess(
                user,
                invoice.OrganizationId);

            var payments = await _PaymentRepo
                .GetTable()
                .Include(x => x.Invoice)
                    .ThenInclude(x => x.Plan)
                .Where(x =>
                    x.CurrentState > 0 &&
                    x.InvoiceId == invoiceId &&
                    x.Invoice != null &&
                    x.Invoice.CurrentState > 0)
                .ToListAsync();

            return _mapper.Map<List<PaymentDto>>(payments);
        }

        // =========================================================
        // Get Payments By Organization
        // =========================================================
        public async Task<List<PaymentDto>> GetByOrganizationIdAsync(
            Guid organizationId)
        {
            if (organizationId == Guid.Empty)
                throw new ArgumentException(
                    "Organization ID is required.",
                    nameof(organizationId));


            var user = await GetCurrentUserAsync();

            EnsurePaymentReadAccess(user);

            var organization =
     await _OrganizationRepo
         .GetTable()
         .FirstOrDefaultAsync(
             x =>
                 x.Id == organizationId &&
                 x.CurrentState > 0);

            if (organization == null)
                throw new KeyNotFoundException(
                    "Organization not found.");

            EnsureOrganizationAccess(
                user,
                organizationId);

            var payments = await _PaymentRepo
                .GetTable()
                .Include(x => x.Invoice)
                    .ThenInclude(x => x.Plan)
               .Where(x =>
    x.CurrentState > 0 &&
    x.Invoice != null &&
    x.Invoice.CurrentState > 0 &&
    x.Invoice.OrganizationId == organizationId)
                .ToListAsync();

            return _mapper.Map<List<PaymentDto>>(payments);
        }

        // =========================================================
        // Process Payment
        // =========================================================
        public async Task<Guid> ProcessPaymentAsync(
      PaymentDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.InvoiceId == Guid.Empty)
                throw new ArgumentException(
                    "Invoice ID is required.",
                    nameof(dto.InvoiceId));

            if (string.IsNullOrWhiteSpace(dto.ProviderReference))
                throw new ArgumentException(
                    "Payment provider reference is required.",
                    nameof(dto.ProviderReference));

            dto.ProviderReference = dto.ProviderReference.Trim();

            var user = await GetCurrentUserAsync();

            EnsurePaymentProcessingAccess(user);

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable);

            try
            {
                // =====================================================
                // Get Invoice Required For Payment
                // =====================================================

                var invoice =
                    await GetInvoiceForPaymentAsync(dto.InvoiceId);

                EnsureOrganizationAccess(
                    user,
                    invoice.OrganizationId);

                // =====================================================
                // Idempotency Check
                // =====================================================

                var existingAttempt =
                    await _PaymentAttemptRepo.GetFirstOrDefaultAsync(
                        x =>
                            x.ProviderReference ==
                            dto.ProviderReference);

                if (existingAttempt != null)
                {
                    var existingPayment =
                        await _PaymentRepo.GetFirstOrDefaultAsync(
                            x =>
                                x.CurrentState > 0 &&
                                x.InvoiceId == invoice.Id &&
                                x.ProviderReference ==
                                dto.ProviderReference);

                    if (existingPayment != null)
                    {
                        await transaction.CommitAsync();

                        return existingPayment.Id;
                    }

                    throw new InvalidOperationException(
                        "This payment provider reference has already been processed.");
                }

                // =====================================================
                // Already Successfully Paid
                // =====================================================

                var successfulPayment =
                    await _PaymentRepo.GetFirstOrDefaultAsync(
                        x =>
                            x.CurrentState > 0 &&
                            x.InvoiceId == invoice.Id &&
                            x.Status == PaymentStatus.Succeeded);

                if (successfulPayment != null)
                {
                    await transaction.CommitAsync();

                    return successfulPayment.Id;
                }

                // =====================================================
                // Validate New Payment
                // =====================================================

                ValidatePaymentRequest(
                    dto,
                    invoice);

                // =====================================================
                // 1. Record Payment Attempt
                // =====================================================

                var attemptedAt = DateTime.UtcNow;

                var paymentAttemptDto = new PaymentAttemptDto
                {
                    InvoiceId = invoice.Id,
                    Amount = invoice.Amount,
                    Status = MapAttemptStatus(dto.Status),
                    ProviderReference = dto.ProviderReference,
                    AttemptedAt = attemptedAt
                };

                await _paymentAttemptService.RecordAsync(
                    paymentAttemptDto);

                // =====================================================
                // 2. Get Current Payment
                // =====================================================

                var payment =
                    await _PaymentRepo.GetFirstOrDefaultAsync(
                        x =>
                            x.CurrentState > 0 &&
                            x.InvoiceId == invoice.Id);

                // =====================================================
                // 3. Create / Update Current Payment
                // =====================================================

                if (payment == null)
                {
                    payment = new TbPayment
                    {
                        Id = Guid.NewGuid(),
                        InvoiceId = invoice.Id,
                        Amount = invoice.Amount,
                        Currency = invoice.Currency,
                        Status = dto.Status,
                        ProviderReference = dto.ProviderReference,
                        PaymentDate = attemptedAt,
                        CurrentState = 1,
                        CreatedDate = attemptedAt
                    };

                    await _PaymentRepo.AddAsync(
                        payment,
                        user.Id);
                }
                else
                {
                    payment.Amount = invoice.Amount;
                    payment.Currency = invoice.Currency;
                    payment.Status = dto.Status;
                    payment.PaymentDate = attemptedAt;
                    payment.ProviderReference = dto.ProviderReference;

                    await _PaymentRepo.UpdateAsync(
                        payment,
                        user.Id);
                }

                // =====================================================
                // 4. Apply Payment Result
                // =====================================================

                await ApplyPaymentResultAsync(
                    invoice,
                    payment,
                    dto.Status,
                    user.Id);

                await transaction.CommitAsync();

                return payment.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================================================
        // Handle Payment Result / Callback
        // =========================================================
        public async Task<bool> HandlePaymentResultAsync(
        PaymentDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.InvoiceId == Guid.Empty)
                throw new ArgumentException(
                    "Invoice ID is required.",
                    nameof(dto.InvoiceId));

            if (string.IsNullOrWhiteSpace(dto.ProviderReference))
                throw new ArgumentException(
                    "Payment provider reference is required.",
                    nameof(dto.ProviderReference));

            dto.ProviderReference = dto.ProviderReference.Trim();

            var user = await GetCurrentUserAsync();

            EnsurePaymentProcessingAccess(user);

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable);

            try
            {
                // =====================================================
                // Get Invoice Required For Payment
                // =====================================================

                var invoice =
                    await GetInvoiceForPaymentAsync(dto.InvoiceId);

                EnsureOrganizationAccess(
                    user,
                    invoice.OrganizationId);

                // =====================================================
                // Idempotency Check
                // =====================================================

                var existingAttempt =
                    await _PaymentAttemptRepo.GetFirstOrDefaultAsync(
                        x =>
                            x.ProviderReference ==
                            dto.ProviderReference);

                if (existingAttempt != null)
                {
                    var existingPayment =
                        await _PaymentRepo.GetFirstOrDefaultAsync(
                            x =>
                                x.CurrentState > 0 &&
                                x.InvoiceId == invoice.Id &&
                                x.ProviderReference ==
                                dto.ProviderReference);

                    if (existingPayment != null)
                    {
                        await transaction.CommitAsync();

                        return true;
                    }

                    throw new InvalidOperationException(
                        "This payment provider reference has already been processed.");
                }

                // =====================================================
                // Already Successfully Paid
                // =====================================================

                var successfulPayment =
                    await _PaymentRepo.GetFirstOrDefaultAsync(
                        x =>
                            x.CurrentState > 0 &&
                            x.InvoiceId == invoice.Id &&
                            x.Status == PaymentStatus.Succeeded);

                if (successfulPayment != null)
                {
                    await transaction.CommitAsync();

                    return true;
                }

                // =====================================================
                // Validate Callback
                // =====================================================

                ValidatePaymentRequest(
                    dto,
                    invoice);

                // =====================================================
                // 1. Record Callback Attempt
                // =====================================================

                var attemptedAt = DateTime.UtcNow;

                var paymentAttemptDto = new PaymentAttemptDto
                {
                    InvoiceId = invoice.Id,
                    Amount = invoice.Amount,
                    Status = MapAttemptStatus(dto.Status),
                    ProviderReference = dto.ProviderReference,
                    AttemptedAt = attemptedAt
                };

                await _paymentAttemptService.RecordAsync(
                    paymentAttemptDto);

                // =====================================================
                // 2. Get Current Payment
                // =====================================================

                var payment =
                    await _PaymentRepo.GetFirstOrDefaultAsync(
                        x =>
                            x.CurrentState > 0 &&
                            x.InvoiceId == invoice.Id);

                // =====================================================
                // 3. Create / Update Current Payment
                // =====================================================

                if (payment == null)
                {
                    payment = new TbPayment
                    {
                        Id = Guid.NewGuid(),
                        InvoiceId = invoice.Id,
                        Amount = invoice.Amount,
                        Currency = invoice.Currency,
                        Status = dto.Status,
                        ProviderReference = dto.ProviderReference,
                        PaymentDate = attemptedAt,
                        CurrentState = 1,
                        CreatedDate = attemptedAt
                    };

                    await _PaymentRepo.AddAsync(
                        payment,
                        user.Id);
                }
                else
                {
                    payment.Amount = invoice.Amount;
                    payment.Currency = invoice.Currency;
                    payment.Status = dto.Status;
                    payment.PaymentDate = attemptedAt;
                    payment.ProviderReference = dto.ProviderReference;

                    await _PaymentRepo.UpdateAsync(
                        payment,
                        user.Id);
                }

                // =====================================================
                // 4. Apply Payment Result
                // =====================================================

                await ApplyPaymentResultAsync(
                    invoice,
                    payment,
                    dto.Status,
                    user.Id);

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================================================
        // Apply Payment Result
        // =========================================================
        private async Task ApplyPaymentResultAsync(
         TbInvoice invoice,
         TbPayment payment,
         PaymentStatus paymentStatus,
         Guid userId)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            if (userId == Guid.Empty)
                throw new ArgumentException(
                    "User ID is required.",
                    nameof(userId));

            // =========================================================
            // Basic Consistency Validation
            // =========================================================

            if (payment.InvoiceId != invoice.Id)
            {
                throw new InvalidOperationException(
                    "Payment does not belong to the invoice.");
            }

            if (payment.Amount != invoice.Amount)
            {
                throw new InvalidOperationException(
                    "Payment amount must match the invoice amount.");
            }

            if (string.IsNullOrWhiteSpace(invoice.Currency))
            {
                throw new InvalidOperationException(
                    "Invoice currency is required.");
            }

            if (string.IsNullOrWhiteSpace(payment.Currency))
            {
                throw new InvalidOperationException(
                    "Payment currency is required.");
            }

            if (!string.Equals(
                    payment.Currency.Trim(),
                    invoice.Currency.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Payment currency must match the invoice currency.");
            }

            if (payment.Status != paymentStatus)
            {
                throw new InvalidOperationException(
                    "Payment status does not match the payment result.");
            }

            if (paymentStatus != PaymentStatus.Pending &&
                paymentStatus != PaymentStatus.Succeeded &&
                paymentStatus != PaymentStatus.Failed)
            {
                throw new InvalidOperationException(
                    "Invalid payment result status.");
            }

            // =========================================================
            // Invoice State Protection
            // =========================================================

            if (invoice.Status != InvoiceStatus.Pending &&
                invoice.Status != InvoiceStatus.PaymentFailed)
            {
                throw new InvalidOperationException(
                    "Payment result can only be applied to a pending or failed invoice.");
            }

            // =========================================================
            // Invoice / Subscription Integrity
            // =========================================================

            if (invoice.SubscriptionId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Invoice is not associated with a subscription.");
            }

            var subscription = invoice.Subscription;

            if (subscription == null ||
                subscription.CurrentState <= 0)
            {
                subscription =
                    await _SubscriptionRepo
                        .GetTable()
                        .FirstOrDefaultAsync(
                            x =>
                                x.Id == invoice.SubscriptionId &&
                                x.CurrentState > 0);
            }

            if (subscription == null)
            {
                throw new KeyNotFoundException(
                    "Active subscription associated with invoice was not found.");
            }

           

            var invoiceTypeName =
                invoice.InvoiceType.ToString();

            var isInitialInvoice =
                string.Equals(
                    invoiceTypeName,
                    nameof(InvoiceType.Initial),
                    StringComparison.OrdinalIgnoreCase);

            var isRenewalInvoice =
                string.Equals(
                    invoiceTypeName,
                    nameof(InvoiceType.Renewal),
                    StringComparison.OrdinalIgnoreCase);

            var isProrationInvoice =
                string.Equals(
                    invoiceTypeName,
                    "Proration",
                    StringComparison.OrdinalIgnoreCase);

            if (!isInitialInvoice &&
                !isRenewalInvoice &&
                !isProrationInvoice)
            {
                throw new InvalidOperationException(
                    "This invoice type cannot be processed through the payment result workflow.");
            }

           

            if (isInitialInvoice)
            {
                if (subscription.Status !=
                    SubscriptionStatus.PendingPayment)
                {
                    throw new InvalidOperationException(
                        "Initial payment can only be processed for a subscription in PendingPayment status.");
                }
            }
            else if (isRenewalInvoice)
            {
                if (subscription.Status != SubscriptionStatus.Active &&
                    subscription.Status != SubscriptionStatus.PastDue &&
                    subscription.Status != SubscriptionStatus.GracePeriod)
                {
                    throw new InvalidOperationException(
                        "Renewal payment can only be processed for an Active, PastDue, or GracePeriod subscription.");
                }
            }

            // =========================================================
            // SUCCESS
            // =========================================================

            if (paymentStatus == PaymentStatus.Succeeded)
            {
                var now = DateTime.UtcNow;

                // =====================================================
                // Initial / Renewal Billing Period
                // =====================================================

                if (isInitialInvoice || isRenewalInvoice)
                {
                    if (!invoice.BillingPeriodId.HasValue ||
                        invoice.BillingPeriodId.Value == Guid.Empty)
                    {
                        throw new InvalidOperationException(
                            "Initial and renewal invoices must have a billing period.");
                    }

                    var billingPeriod =
                        await _context.TbBillingPeriods
                            .AsNoTracking()
                            .FirstOrDefaultAsync(
                                x =>
                                    x.Id == invoice.BillingPeriodId.Value &&
                                    x.CurrentState > 0);

                    if (billingPeriod == null)
                    {
                        throw new KeyNotFoundException(
                            "Billing period associated with invoice was not found.");
                    }

                    if (billingPeriod.SubscriptionId != subscription.Id)
                    {
                        throw new InvalidOperationException(
                            "Billing period does not belong to the invoice subscription.");
                    }

                    if (billingPeriod.Start >= billingPeriod.End)
                    {
                        throw new InvalidOperationException(
                            "Billing period start must be earlier than billing period end.");
                    }

                   

                    if (isRenewalInvoice)
                    {
                        var overlappingPeriod =
                            await _context.TbBillingPeriods
                                .AsNoTracking()
                                .AnyAsync(
                                    x =>
                                        x.Id != billingPeriod.Id &&
                                        x.SubscriptionId == subscription.Id &&
                                        x.CurrentState > 0 &&
                                        x.Start < billingPeriod.End &&
                                        x.End > billingPeriod.Start);

                        if (overlappingPeriod)
                        {
                            throw new InvalidOperationException(
                                "Renewal billing period overlaps another billing period of the subscription.");
                        }

                        // -------------------------------------------------
                        // Must not overlap the currently active period
                        // -------------------------------------------------

                        if (subscription.CurrentPeriodEnd.HasValue &&
                            billingPeriod.Start <
                            subscription.CurrentPeriodEnd.Value)
                        {
                            throw new InvalidOperationException(
                                "Renewal billing period cannot overlap the current subscription period.");
                        }
                    }

                    // -------------------------------------------------
                    // Historical Billing Period Is Authoritative
                    // -------------------------------------------------

                    subscription.CurrentPeriodStart =
                        billingPeriod.Start;

                    subscription.CurrentPeriodEnd =
                        billingPeriod.End;
                }

                // =====================================================
                // Invoice -> Paid
                // =====================================================

                invoice.Status =
                    InvoiceStatus.Paid;

                await _InvoiceRepo.UpdateAsync(
                    invoice,
                    userId);

                // =====================================================
                // Subscription State Changes
                // =====================================================

                if (isInitialInvoice)
                {
                    

                    subscription.Status =
                        SubscriptionStatus.Active;

                    if (!subscription.StartedAt.HasValue)
                    {
                        subscription.StartedAt = now;
                    }
                }
                else if (isRenewalInvoice)
                {
                   

                    if (subscription.Status ==
                            SubscriptionStatus.PastDue ||
                        subscription.Status ==
                            SubscriptionStatus.GracePeriod)
                    {
                        subscription.Status =
                            SubscriptionStatus.Active;
                    }
                }
                else if (isProrationInvoice)
                {
                   
                }

                await _SubscriptionRepo.UpdateAsync(
                    subscription,
                    userId);

                // =====================================================
                // Success Notification
                // =====================================================

                var notificationType =
                    isRenewalInvoice
                        ? NotificationType.RenewalSucceeded
                        : NotificationType.PaymentSucceeded;

                var notificationDescription =
                    isRenewalInvoice
                        ? $"Renewal payment succeeded for Invoice {invoice.Id}."
                        : $"Payment succeeded for Invoice {invoice.Id}.";

                var successNotification = new NotificationDto
                {
                    OrganizationId = invoice.OrganizationId,
                    UserId = null,
                    Description = notificationDescription,
                    RelatedEntityType = EntityType.Payment,
                    RelatedEntityId = payment.Id,
                    IsRead = false,
                    Type = notificationType
                };

                await _notificationService.AddAsync(
                    successNotification);

                // =====================================================
                // Success Audit
                // =====================================================

                var successAction =
                    isRenewalInvoice
                        ? "RenewalSucceeded"
                        : "PaymentSucceeded";

                var successAudit = new AuditLogDto
                {
                    UserId = userId,
                    Action = successAction,
                    EntityType = EntityType.Payment,
                    EntityId = payment.Id,
                    Metadata = JsonSerializer.Serialize(
                        new
                        {
                            InvoiceId = invoice.Id,
                            PaymentId = payment.Id,
                            Amount = payment.Amount,
                            Currency = payment.Currency,
                            Status = payment.Status.ToString(),
                            InvoiceType = invoice.InvoiceType.ToString(),
                            SubscriptionStatus =
                                subscription.Status.ToString(),
                            BillingPeriodId =
                                invoice.BillingPeriodId
                        }),
                    CorrelationId = null,
                    Timestamp = now
                };

                await _auditLogService.AddAsync(
                    successAudit);

                return;
            }

            // =========================================================
            // FAILED
            // =========================================================

            if (paymentStatus == PaymentStatus.Failed)
            {
                

                invoice.Status =
                    InvoiceStatus.PaymentFailed;

                await _InvoiceRepo.UpdateAsync(
                    invoice,
                    userId);

                if (isRenewalInvoice &&
                    subscription.Status == SubscriptionStatus.Active)
                {
                    subscription.Status =
                        SubscriptionStatus.PastDue;

                    await _SubscriptionRepo.UpdateAsync(
                        subscription,
                        userId);
                }

                // =====================================================
                // Failure Notification
                // =====================================================

                var failureNotification = new NotificationDto
                {
                    OrganizationId = invoice.OrganizationId,
                    UserId = null,
                    Description =
                        $"Payment failed for Invoice {invoice.Id}.",
                    RelatedEntityType = EntityType.Payment,
                    RelatedEntityId = payment.Id,
                    IsRead = false,
                    Type = NotificationType.PaymentFailed
                };

                await _notificationService.AddAsync(
                    failureNotification);

                // =====================================================
                // Failure Audit
                // =====================================================

                var failureAudit = new AuditLogDto
                {
                    UserId = userId,
                    Action = "PaymentFailed",
                    EntityType = EntityType.Payment,
                    EntityId = payment.Id,
                    Metadata = JsonSerializer.Serialize(
                        new
                        {
                            InvoiceId = invoice.Id,
                            PaymentId = payment.Id,
                            Amount = payment.Amount,
                            Currency = payment.Currency,
                            Status = payment.Status.ToString(),
                            InvoiceType = invoice.InvoiceType.ToString(),
                            SubscriptionStatus =
                                subscription.Status.ToString()
                        }),
                    CorrelationId = null,
                    Timestamp = DateTime.UtcNow
                };

                await _auditLogService.AddAsync(
                    failureAudit);

                return;
            }

            // =========================================================
            // PENDING
            // =========================================================

            if (paymentStatus == PaymentStatus.Pending)
            {
               

                invoice.Status =
                    InvoiceStatus.Pending;

                await _InvoiceRepo.UpdateAsync(
                    invoice,
                    userId);

                return;
            }

            // =========================================================
            // Unsupported Result
            // =========================================================

            throw new InvalidOperationException(
                "This payment status cannot be processed through the payment result workflow.");
        }

        // =========================================================
        // Get Invoice Required For Payment
        // =========================================================
        private async Task<TbInvoice> GetInvoiceForPaymentAsync(
        Guid invoiceId)
        {
            if (invoiceId == Guid.Empty)
                throw new ArgumentException(
                    "Invoice ID is required.",
                    nameof(invoiceId));

            var invoice = await _InvoiceRepo
                .GetTable()
                .Include(x => x.Subscription)
                .Include(x => x.Plan)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == invoiceId &&
                        x.CurrentState > 0);

            if (invoice == null)
            {
                throw new KeyNotFoundException(
                    "Invoice not found.");
            }

            if (invoice.SubscriptionId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Invoice is not associated with a subscription.");
            }

            if (invoice.Subscription == null ||
                invoice.Subscription.CurrentState <= 0)
            {
                throw new InvalidOperationException(
                    "The subscription associated with the invoice is not active.");
            }

            return invoice;
        }

        // =========================================================
        // Validate Payment Request
        // =========================================================
        private void ValidatePaymentRequest(
     PaymentDto dto,
     TbInvoice invoice)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            // =====================================================
            // Payment Provider Reference
            // =====================================================

            if (string.IsNullOrWhiteSpace(dto.ProviderReference))
            {
                throw new ArgumentException(
                    "Payment provider reference is required.",
                    nameof(dto.ProviderReference));
            }

            // =====================================================
            // Invoice Status
            // =====================================================

            if (invoice.Status != InvoiceStatus.Pending &&
                invoice.Status != InvoiceStatus.PaymentFailed)
            {
                throw new InvalidOperationException(
                    "Payment can only be processed for a pending or failed invoice.");
            }

            // =====================================================
            // Invoice Amount
            // =====================================================

            if (invoice.Amount <= 0)
            {
                throw new InvalidOperationException(
                    "Invoice amount must be greater than zero.");
            }

            // =====================================================
            // Payment Amount
            // =====================================================

            if (dto.Amount != invoice.Amount)
            {
                throw new InvalidOperationException(
                    "Payment amount must match the invoice amount.");
            }

            // =====================================================
            // Currency
            // =====================================================

            if (string.IsNullOrWhiteSpace(invoice.Currency))
            {
                throw new InvalidOperationException(
                    "Invoice currency is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Currency))
            {
                throw new ArgumentException(
                    "Payment currency is required.",
                    nameof(dto.Currency));
            }

            if (!string.Equals(
                    dto.Currency.Trim(),
                    invoice.Currency.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Payment currency must match the invoice currency.");
            }

            // =====================================================
            // Allowed Payment Result States
            // =====================================================

            if (dto.Status != PaymentStatus.Pending &&
                dto.Status != PaymentStatus.Succeeded &&
                dto.Status != PaymentStatus.Failed)
            {
                throw new InvalidOperationException(
                    "Invalid payment result status.");
            }
        }

        // =========================================================
        // Current User
        // =========================================================
        private async Task<UserDto> GetCurrentUserAsync()
        {
            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");
            }

            var user =
                await _userService.GetUserByIdAsync(
                    userId.ToString());

            if (user == null)
            {
                throw new UnauthorizedAccessException(
                    "User not found.");
            }

            if (user.Id == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "Authenticated user is invalid.");
            }

            return user;
        }





        // =========================================================
        // Tenant Isolation
        // =========================================================
        private void EnsurePaymentReadAccess(
     UserDto user)
        {
            var isPlatformAdmin =
                string.Equals(
                    user.Role,
                    "Platform Admin",
                    StringComparison.OrdinalIgnoreCase);

            var isCustomerAdmin =
                string.Equals(
                    user.Role,
                    "Customer Admin",
                    StringComparison.OrdinalIgnoreCase);

            if (!isPlatformAdmin && !isCustomerAdmin)
            {
                throw new ForbiddenAccessException(
                    "Only Platform Admin or Customer Admin can access payment data.");
            }
        }

        private void EnsurePaymentProcessingAccess(
            UserDto user)
        {
            var isPlatformAdmin =
                string.Equals(
                    user.Role,
                    "Platform Admin",
                    StringComparison.OrdinalIgnoreCase);

            var isCustomerAdmin =
                string.Equals(
                    user.Role,
                    "Customer Admin",
                    StringComparison.OrdinalIgnoreCase);

            if (!isPlatformAdmin && !isCustomerAdmin)
            {
                throw new ForbiddenAccessException(
                    "Only Platform Admin or Customer Admin can process payments.");
            }
        }

        // =========================================================
        // Tenant Isolation
        // =========================================================
        private void EnsureOrganizationAccess(
            UserDto user,
            Guid? organizationId)
        {
            if (organizationId == null)
            {
                throw new InvalidOperationException(
                    "Payment organization was not found.");
            }

            var isPlatformAdmin =
                string.Equals(
                    user.Role,
                    "Platform Admin",
                    StringComparison.OrdinalIgnoreCase);

            if (isPlatformAdmin)
                return;

            if (user.OrganizationId != organizationId.Value)
            {
                throw new ForbiddenAccessException(
                    "You are not authorized to access this organization's payment data.");
            }
        }

        // =========================================================
        // PaymentAttempt Status Mapping
        // =========================================================
        private PaymentAttemptStatus MapAttemptStatus(
            PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending =>
                    PaymentAttemptStatus.Pending,

                PaymentStatus.Succeeded =>
                    PaymentAttemptStatus.Succeeded,

                PaymentStatus.Failed =>
                    PaymentAttemptStatus.Failed,

                _ =>
                    throw new InvalidOperationException(
                        "Invalid payment attempt status.")
            };
        }
    }
}