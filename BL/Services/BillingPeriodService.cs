using AutoMapper;
using BL.Contract;
using BL.Dtos;
using BL.Exceptions;
using DAL.Contracts;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BL.Services
{
    public class BillingPeriodService
        : BaseService<TbBillingPeriod, BillingPeriodDto>, IBillingPeriod
    {
        private readonly ITableRepository<TbBillingPeriod> _BillingPeriodRepo;
        private readonly ITableRepository<TbSubscription> _SubscriptionRepo;
        private readonly ITableRepository<TbPlan> _PlanRepo;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public BillingPeriodService(
            ITableRepository<TbBillingPeriod> tableRepository,
            ITableRepository<TbSubscription> subscriptionRepository,
            ITableRepository<TbPlan> planRepository,
            IMapper mapper,
            IUserService userService)
            : base(tableRepository, mapper, userService)
        {
            _BillingPeriodRepo = tableRepository;
            _SubscriptionRepo = subscriptionRepository;
            _PlanRepo = planRepository;
            _mapper = mapper;
            _userService = userService;
        }

        // =========================================================
        // Get Billing Period By Id
        // =========================================================
        public async Task<BillingPeriodDto> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(
                    "Billing period ID is required.");

            var user = await GetAuthenticatedUserAsync();

            var billingPeriod = await _BillingPeriodRepo
                .GetTable()
                .AsNoTracking()
                .Include(x => x.Subscription)
                    .ThenInclude(x => x.Organization)
                .Include(x => x.Plan)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.CurrentState > 0);

            if (billingPeriod == null)
                throw new KeyNotFoundException(
                    "Billing period was not found.");

            if (billingPeriod.Subscription == null)
                throw new InvalidOperationException(
                    "The billing period is not associated with a subscription.");

            await EnsureOrganizationAccessAsync(
                billingPeriod.Subscription.OrganizationId,
                user);

            return _mapper.Map<BillingPeriodDto>(billingPeriod);
        }

        // =========================================================
        // Get All Billing Periods For A Subscription
        // =========================================================
        public async Task<IEnumerable<BillingPeriodDto>>
            GetBySubscriptionIdAsync(Guid subscriptionId)
        {
            if (subscriptionId == Guid.Empty)
                throw new ArgumentException(
                    "Subscription ID is required.");

            var user = await GetAuthenticatedUserAsync();

            var subscription = await _SubscriptionRepo
                .GetTable()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == subscriptionId &&
                    x.CurrentState > 0);

            if (subscription == null)
                throw new KeyNotFoundException(
                    "Subscription was not found.");

            await EnsureOrganizationAccessAsync(
                subscription.OrganizationId,
                user);

            var billingPeriods = await _BillingPeriodRepo
                .GetTable()
                .AsNoTracking()
                .Include(x => x.Subscription)
                    .ThenInclude(x => x.Organization)
                .Include(x => x.Plan)
                .Where(x =>
                    x.SubscriptionId == subscriptionId &&
                    x.CurrentState > 0)
                .OrderByDescending(x => x.Start)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BillingPeriodDto>>(
                billingPeriods);
        }

      
        public async Task<Guid> AddAsync(BillingPeriodDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var user = await GetAuthenticatedUserAsync();

            // =====================================================
            // Validate Subscription
            // =====================================================
            if (dto.SubscriptionId == Guid.Empty)
                throw new ArgumentException(
                    "Subscription ID is required.");

            var subscription = await _SubscriptionRepo
                .GetTable()
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.SubscriptionId &&
                    x.CurrentState > 0);

            if (subscription == null)
                throw new KeyNotFoundException(
                    "Subscription was not found.");

            await EnsureOrganizationAccessAsync(
                subscription.OrganizationId,
                user);

            // =====================================================
            // Validate Plan
            // =====================================================
            if (dto.PlanId == Guid.Empty)
                throw new ArgumentException(
                    "Plan ID is required.");

            if (subscription.PlanId != dto.PlanId)
                throw new InvalidOperationException(
                    "The billing period plan must match the subscription plan.");

         
            var plan = await _PlanRepo
                .GetTable()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.PlanId &&
                    x.CurrentState > 0);

            if (plan == null)
                throw new KeyNotFoundException(
                    "The subscription plan was not found.");

            // =====================================================
            // Validate Configured Duration
            // =====================================================
            if (plan.Duration <= 0)
                throw new InvalidOperationException(
                    "The subscription plan has an invalid billing duration.");

            // =====================================================
            // Validate Dates
            // =====================================================
            if (dto.Start == default)
                throw new ArgumentException(
                    "Billing period start date is required.");

            if (dto.End == default)
                throw new ArgumentException(
                    "Billing period end date is required.");

            if (dto.Start.Kind != DateTimeKind.Utc ||
                dto.End.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Billing period dates must be provided in UTC.");
            }

            if (dto.End <= dto.Start)
                throw new ArgumentException(
                    "Billing period end date must be greater than start date.");

          
            DateTime expectedEnd = dto.Start.AddDays(plan.Duration);

            if (dto.End != expectedEnd)
            {
                throw new InvalidOperationException(
                    $"Billing period must be exactly {plan.Duration} days from the start date.");
            }

            // =====================================================
            // Validate Amount
            // =====================================================
            if (dto.Amount <= 0)
                throw new ArgumentException(
                    "Billing period amount must be greater than zero.");

            // =====================================================
            // Validate Currency
            // =====================================================
            if (string.IsNullOrWhiteSpace(dto.Currency))
                throw new ArgumentException(
                    "Currency is required.");

            string currency = dto.Currency
                .Trim()
                .ToUpperInvariant();

            if (currency.Length != 3)
                throw new ArgumentException(
                    "Currency must be exactly 3 characters.");

            // =====================================================
            // Prevent Overlapping Billing Periods
            // =====================================================
            bool hasOverlap = await _BillingPeriodRepo
                .GetTable()
                .AnyAsync(x =>
                    x.SubscriptionId == dto.SubscriptionId &&
                    x.CurrentState > 0 &&
                    dto.Start < x.End &&
                    dto.End > x.Start);

            if (hasOverlap)
                throw new InvalidOperationException(
                    "The billing period overlaps with an existing billing period.");

            // =====================================================
            // Create Historical Billing Period
            // =====================================================
            var entity = _mapper.Map<TbBillingPeriod>(dto);

           
            entity.Id = Guid.NewGuid();

          
            entity.Status = BillingPeriodStatus.Active;

           
            entity.Subscription = null!;
            entity.Plan = null!;
            entity.Invoice = new HashSet<TbInvoice>();

            
            entity.Currency = currency;

            await _BillingPeriodRepo.AddAsync(
                entity,
                user.Id);

            return entity.Id;
        }

        // =========================================================
        // Get Authenticated User
        // =========================================================
        private async Task<UserDto> GetAuthenticatedUserAsync()
        {
            Guid loggedInUserId =
                _userService.GetLoggedInUser();

            if (loggedInUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            var user =
                await _userService.GetUserByIdAsync(
                    loggedInUserId.ToString());

            if (user == null)
                throw new UnauthorizedAccessException(
                    "Authenticated user was not found.");

            return user;
        }

        // =========================================================
        // Tenant / Organization Access
        // =========================================================
        private Task EnsureOrganizationAccessAsync(
            Guid organizationId,
            UserDto user)
        {
            if (string.IsNullOrWhiteSpace(user.Role) &&
                (user.Roles == null || user.Roles.Count == 0))
            {
                throw new UnauthorizedAccessException(
                    "User role information is unavailable.");
            }

            bool isPlatformAdmin =
                string.Equals(
                    user.Role,
                    "Platform Admin",
                    StringComparison.OrdinalIgnoreCase)
                ||
                (user.Roles != null &&
                 user.Roles.Any(r =>
                     string.Equals(
                         r,
                         "Platform Admin",
                         StringComparison.OrdinalIgnoreCase)));

          
            if (isPlatformAdmin)
                return Task.CompletedTask;

            if (!user.OrganizationId.HasValue)
            {
                throw new UnauthorizedAccessException(
                    "User is not associated with an organization.");
            }

          
            if (organizationId != user.OrganizationId.Value)
            {
                throw new ForbiddenAccessException(
                    "You are not authorized to access this organization's billing data.");
            }

            return Task.CompletedTask;
        }
    }
}