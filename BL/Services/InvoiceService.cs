using AutoMapper;
using BL.Contract;
using BL.Dtos;
using BL.Exceptions;
using DAL.Contracts;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BL.Services
{
    public class InvoiceService : BaseService<TbInvoice, InvoiceDto>, IInvoice
    {
        private readonly ITableRepository<TbInvoice> _invoiceRepo;
        private readonly ITableRepository<TbSubscription> _subscriptionRepo;
        private readonly ITableRepository<TbOrganization> _organizationRepo;
        private readonly ITableRepository<TbPlan> _planRepo;
        private readonly ITableRepository<TbBillingPeriod> _billingPeriodRepo;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public InvoiceService(
            ITableRepository<TbInvoice> invoiceRepo,
            ITableRepository<TbSubscription> subscriptionRepo,
            ITableRepository<TbOrganization> organizationRepo,
            ITableRepository<TbPlan> planRepo,
            ITableRepository<TbBillingPeriod> billingPeriodRepo,
            IMapper mapper,
            IUserService userService)
            : base(invoiceRepo, mapper, userService)
        {
            _invoiceRepo = invoiceRepo;
            _subscriptionRepo = subscriptionRepo;
            _organizationRepo = organizationRepo;
            _planRepo = planRepo;
            _billingPeriodRepo = billingPeriodRepo;
            _mapper = mapper;
            _userService = userService;
        }

        // ============================================================
        // GET BY ID
        // ============================================================

        public override async Task<InvoiceDto?> GetByIdAsync(Guid id)
        {

            var currentUser = await GetCurrentUserAsync();

            if (currentUser == null)
                return null;

            var isPlatformAdmin = IsPlatformAdmin(currentUser);
            var isCustomerAdmin = IsCustomerAdmin(currentUser);

            // Customer User cannot access invoices.
            if (!isPlatformAdmin && !isCustomerAdmin)
                return null;

            var invoice = await GetInvoiceWithRelationsAsync(id);

            if (invoice == null)
                return null;

            // Server-side tenant isolation.
            if (!isPlatformAdmin)
            {
                if (!currentUser.OrganizationId.HasValue ||
                    invoice.OrganizationId != currentUser.OrganizationId.Value)
                {
                    return null;
                }
            }

            return _mapper.Map<InvoiceDto>(invoice);
        }

        // ============================================================
        // GET BY SUBSCRIPTION ID
        // ============================================================

        public async Task<IEnumerable<InvoiceDto>> GetBySubscriptionIdAsync(
            Guid subscriptionId)
        {
            var currentUser = await GetCurrentUserAsync();

            if (currentUser == null)
                return Enumerable.Empty<InvoiceDto>();

            var isPlatformAdmin = IsPlatformAdmin(currentUser);
            var isCustomerAdmin = IsCustomerAdmin(currentUser);

            // Customer User cannot access invoices.
            if (!isPlatformAdmin && !isCustomerAdmin)
                return Enumerable.Empty<InvoiceDto>();

            var subscription =
                await _subscriptionRepo.GetByIdAsync(subscriptionId);

            if (subscription == null ||
                subscription.CurrentState <= 0)
            {
                return Enumerable.Empty<InvoiceDto>();
            }

            if (!isPlatformAdmin)
            {
                if (!currentUser.OrganizationId.HasValue ||
                    subscription.OrganizationId != currentUser.OrganizationId.Value)
                {
                    return Enumerable.Empty<InvoiceDto>();
                }
            }

            var invoices = await _invoiceRepo
                .GetTable()
                .Where(x =>
                    x.SubscriptionId == subscriptionId &&
                    x.CurrentState > 0)
                .Include(x => x.Organization)
                .Include(x => x.Subscription)
                    .ThenInclude(x => x.Organization)
                .Include(x => x.Subscription)
                    .ThenInclude(x => x.Plan)
                .Include(x => x.Plan)
                .Include(x => x.BillingPeriod)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        }

        // ============================================================
        // GET BY ORGANIZATION ID
        // ============================================================

        public async Task<IEnumerable<InvoiceDto>> GetByOrganizationIdAsync(
            Guid organizationId)
        {
            var currentUser = await GetCurrentUserAsync();

            if (currentUser == null)
                return Enumerable.Empty<InvoiceDto>();

            var isPlatformAdmin = IsPlatformAdmin(currentUser);
            var isCustomerAdmin = IsCustomerAdmin(currentUser);

            // Customer User cannot access invoices.
            if (!isPlatformAdmin && !isCustomerAdmin)
                return Enumerable.Empty<InvoiceDto>();

            if (!isPlatformAdmin)
            {
                if (!currentUser.OrganizationId.HasValue ||
                    currentUser.OrganizationId.Value != organizationId)
                {
                    return Enumerable.Empty<InvoiceDto>();
                }
            }

            var organization =
                await _organizationRepo.GetByIdAsync(organizationId);

            if (organization == null ||
                organization.CurrentState <= 0)
            {
                return Enumerable.Empty<InvoiceDto>();
            }

            var invoices = await _invoiceRepo
                .GetTable()
                .Where(x =>
                    x.OrganizationId == organizationId &&
                    x.CurrentState > 0)
                .Include(x => x.Organization)
                .Include(x => x.Subscription)
                    .ThenInclude(x => x.Organization)
                .Include(x => x.Subscription)
                    .ThenInclude(x => x.Plan)
                .Include(x => x.Plan)
                .Include(x => x.BillingPeriod)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        }

        // ============================================================
        // CREATE PENDING INVOICE
        // ============================================================

        public new async Task<Guid> AddAsync(InvoiceDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var currentUser = await GetCurrentUserAsync();

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException(
                    "Authenticated user was not found.");
            }

            var isPlatformAdmin = IsPlatformAdmin(currentUser);
            var isCustomerAdmin = IsCustomerAdmin(currentUser);

            // Only Platform Admin and Customer Admin can create invoices.
            if (!isPlatformAdmin && !isCustomerAdmin)
            {
                throw new ForbiddenAccessException(
                    "You are not authorized to create an invoice.");
            }

            // ============================================================
            // 1. Validate Invoice Type
            // ============================================================

            if (!Enum.IsDefined(typeof(InvoiceType), dto.InvoiceType))
            {
                throw new InvalidOperationException(
                    "Invalid invoice type.");
            }

         
            if (dto.InvoiceType == InvoiceType.Proration)
            {
                throw new InvalidOperationException(
                    "Proration invoices must be created through the subscription upgrade workflow.");
            }

            if (dto.InvoiceType != InvoiceType.Initial &&
                dto.InvoiceType != InvoiceType.Renewal)
            {
                throw new InvalidOperationException(
                    "Only Initial and Renewal invoices can be created by this operation.");
            }

            // ============================================================
            // 2. Billing Period is mandatory
            // ============================================================

            if (!dto.BillingPeriodId.HasValue ||
                dto.BillingPeriodId.Value == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Billing period is required when creating an invoice.");
            }

            // ============================================================
            // 3. Subscription is mandatory
            // ============================================================

            if (dto.SubscriptionId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Subscription is required when creating an invoice.");
            }

            var subscription =
                await _subscriptionRepo.GetByIdAsync(dto.SubscriptionId);

            if (subscription == null ||
                subscription.CurrentState <= 0)
            {
                throw new InvalidOperationException(
                    "Subscription was not found.");
            }

            // ============================================================
            // 4. Server-side Tenant Isolation
            // ============================================================

            if (!isPlatformAdmin)
            {
                if (!currentUser.OrganizationId.HasValue)
                {
                    throw new ForbiddenAccessException(
                        "You are not authorized to create an invoice.");
                }

                if (subscription.OrganizationId !=
                    currentUser.OrganizationId.Value)
                {
                    throw new ForbiddenAccessException(
                        "You are not authorized to create an invoice for this organization.");
                }
            }

            // ============================================================
            // 5. Subscription Organization
            // ============================================================

            if (subscription.OrganizationId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Subscription organization is required.");
            }

            var organization =
                await _organizationRepo.GetByIdAsync(
                    subscription.OrganizationId);

            if (organization == null ||
                organization.CurrentState <= 0)
            {
                throw new InvalidOperationException(
                    "Subscription organization was not found.");
            }

           

            if (subscription.PlanId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Subscription plan is required.");
            }

            var plan =
                await _planRepo.GetByIdAsync(subscription.PlanId);

            if (plan == null)
            {
                throw new InvalidOperationException(
                    "Subscription plan was not found.");
            }

            // ============================================================
            // 7. Billing Period
            // ============================================================

            var billingPeriod =
                await _billingPeriodRepo.GetByIdAsync(
                    dto.BillingPeriodId.Value);

            if (billingPeriod == null ||
                billingPeriod.CurrentState <= 0)
            {
                throw new InvalidOperationException(
                    "Billing period was not found.");
            }

            if (billingPeriod.SubscriptionId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Billing period subscription is required.");
            }

            // ============================================================
            // 8. Billing Period must belong to Subscription
            // ============================================================

            if (billingPeriod.SubscriptionId != subscription.Id)
            {
                throw new InvalidOperationException(
                    "The billing period does not belong to the subscription.");
            }

            // ============================================================
            // 9. Billing Period Plan must match Subscription Plan
            // ============================================================

            if (billingPeriod.PlanId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Billing period plan is required.");
            }

            if (billingPeriod.PlanId != subscription.PlanId)
            {
                throw new InvalidOperationException(
                    "The billing period plan does not match the subscription plan.");
            }

            // ============================================================
            // 10. Validate Billing Period Dates
            // ============================================================

            if (billingPeriod.Start >= billingPeriod.End)
            {
                throw new InvalidOperationException(
                    "Billing period end date must be greater than start date.");
            }

            // ============================================================
            // 11. Exact Timestamp-Based Duration
            // ============================================================

            var exactDuration =
                (billingPeriod.End - billingPeriod.Start).TotalDays;

            if (exactDuration <= 0)
            {
                throw new InvalidOperationException(
                    "Billing period duration must be greater than zero.");
            }

            if (exactDuration != Math.Truncate(exactDuration))
            {
                throw new InvalidOperationException(
                    "Billing period duration must represent a whole number of days.");
            }

            int duration;

            try
            {
                duration = checked((int)exactDuration);
            }
            catch (OverflowException)
            {
                throw new InvalidOperationException(
                    "Billing period duration is outside the supported range.");
            }

            if (duration <= 0)
            {
                throw new InvalidOperationException(
                    "Invoice duration must be greater than zero.");
            }

            

            var existingInvoice =
                await _invoiceRepo
                    .GetTable()
                    .Where(x =>
                        x.CurrentState > 0 &&
                        x.SubscriptionId == subscription.Id &&
                        x.BillingPeriodId.HasValue &&
                        x.BillingPeriodId.Value == billingPeriod.Id &&
                        x.InvoiceType == dto.InvoiceType)
                    .FirstOrDefaultAsync();

            if (existingInvoice != null)
            {
                return existingInvoice.Id;
            }

            // ============================================================
            // 13. Server-Authoritative Financial Values
            // ============================================================

            var amount = billingPeriod.Amount;

            if (amount <= 0)
            {
                throw new InvalidOperationException(
                    "Billing period amount must be greater than zero.");
            }

            var currency = billingPeriod.Currency;

            if (string.IsNullOrWhiteSpace(currency) ||
                currency.Trim().Length != 3)
            {
                throw new InvalidOperationException(
                    "Billing period currency must be a valid 3-character currency code.");
            }

            currency = currency.Trim().ToUpperInvariant();

            // ============================================================
            // 14. Historical Plan Snapshot
            // ============================================================

            var planNameSnapshot = plan.PlanName;

            if (string.IsNullOrWhiteSpace(planNameSnapshot))
            {
                throw new InvalidOperationException(
                    "Plan name snapshot is required.");
            }

            var planUserLimitSnapshot = plan.MaxUsers;

            if (planUserLimitSnapshot <= 0)
            {
                throw new InvalidOperationException(
                    "Plan user limit snapshot must be greater than zero.");
            }

            planNameSnapshot = planNameSnapshot.Trim();

            

            var invoice = new TbInvoice
            {
                Id = Guid.NewGuid(),

                CurrentState = 1,
                CreatedDate = DateTime.UtcNow,

                OrganizationId = subscription.OrganizationId,
                SubscriptionId = subscription.Id,
                PlanId = subscription.PlanId,

                BillingPeriodId = billingPeriod.Id,

                StartDate = billingPeriod.Start,
                EndDate = billingPeriod.End,
                Duration = duration,

                PlanNameSnapshot = planNameSnapshot,
                PlanUserLimitSnapshot = planUserLimitSnapshot,

                Amount = amount,
                Currency = currency,

                Status = InvoiceStatus.Pending,

                InvoiceType = dto.InvoiceType
            };

            // ============================================================
            // 16. Final Domain Validation
            // ============================================================

            if (invoice.Id == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Invoice ID is required.");
            }

            if (invoice.OrganizationId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Invoice organization is required.");
            }

            if (invoice.SubscriptionId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Invoice subscription is required.");
            }

            if (invoice.PlanId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Invoice plan is required.");
            }

            if (!invoice.BillingPeriodId.HasValue ||
                invoice.BillingPeriodId.Value == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Invoice billing period is required.");
            }

            if (invoice.StartDate >= invoice.EndDate)
            {
                throw new InvalidOperationException(
                    "Invoice end date must be greater than start date.");
            }

            if (invoice.Duration <= 0)
            {
                throw new InvalidOperationException(
                    "Invoice duration must be greater than zero.");
            }

            if (invoice.Amount <= 0)
            {
                throw new InvalidOperationException(
                    "Invoice amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(invoice.Currency) ||
                invoice.Currency.Trim().Length != 3)
            {
                throw new InvalidOperationException(
                    "Invoice currency must be a valid 3-character currency code.");
            }

            if (string.IsNullOrWhiteSpace(invoice.PlanNameSnapshot))
            {
                throw new InvalidOperationException(
                    "Invoice plan name snapshot is required.");
            }

            if (invoice.PlanUserLimitSnapshot <= 0)
            {
                throw new InvalidOperationException(
                    "Invoice plan user limit snapshot must be greater than zero.");
            }

            if (!Enum.IsDefined(typeof(InvoiceType), invoice.InvoiceType))
            {
                throw new InvalidOperationException(
                    "Invalid invoice type.");
            }

            if (invoice.InvoiceType != InvoiceType.Initial &&
                invoice.InvoiceType != InvoiceType.Renewal)
            {
                throw new InvalidOperationException(
                    "Only Initial and Renewal invoices can be created by this operation.");
            }

            if (!Enum.IsDefined(typeof(InvoiceStatus), invoice.Status))
            {
                throw new InvalidOperationException(
                    "Invalid invoice status.");
            }

            if (invoice.Status != InvoiceStatus.Pending)
            {
                throw new InvalidOperationException(
                    "A newly created invoice must start with Pending status.");
            }

            // ============================================================
            // 17. Persist ONLY the Invoice
            // ============================================================

            await _invoiceRepo.AddAsync(
                invoice,
                currentUser.Id);

            return invoice.Id;
        }
        // ============================================================
        // PRIVATE HELPERS
        // ============================================================

        private async Task<UserDto?> GetCurrentUserAsync()
        {
            var currentUserId = _userService.GetLoggedInUser();

            return await _userService.GetUserByIdAsync(
                currentUserId.ToString());
        }

        private async Task<TbInvoice?> GetInvoiceWithRelationsAsync(
            Guid id)
        {
            return await _invoiceRepo
                .GetTable()
                .Where(x =>
                    x.Id == id &&
                    x.CurrentState > 0)
                .Include(x => x.Organization)
                .Include(x => x.Subscription)
                    .ThenInclude(x => x.Organization)
                .Include(x => x.Subscription)
                    .ThenInclude(x => x.Plan)
                .Include(x => x.Plan)
                .Include(x => x.BillingPeriod)
                .FirstOrDefaultAsync();
        }

        private static bool IsPlatformAdmin(UserDto user)
        {
            return user.Roles.Any(r =>
                string.Equals(
                    r,
                    "Platform Admin",
                    StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsCustomerAdmin(UserDto user)
        {
            return user.Roles.Any(r =>
                string.Equals(
                    r,
                    "Customer Admin",
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}