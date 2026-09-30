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
    public class PendingPlanChangeService : BaseService<TbPendingPlanChange, PendingPlanChangeDto>, IPendingPlanChange
    {
        private readonly ITableRepository<TbPendingPlanChange> _PendingPlanChangeRepo;
        private readonly ITableRepository<TbSubscription> _SubscriptionRepo;
        private readonly ITableRepository<TbPlan> _PlanRepo;
        private readonly IMapper _Mapper;
        private readonly IUserService _userService;

        public PendingPlanChangeService(
            ITableRepository<TbPendingPlanChange> tableRepository,
            ITableRepository<TbSubscription> tableRepository1,
            ITableRepository<TbPlan> tableRepository2,
            IMapper mapper,
            IUserService userService)
            : base(tableRepository, mapper, userService)
        {
            _PendingPlanChangeRepo = tableRepository;
            _SubscriptionRepo = tableRepository1;
            _PlanRepo = tableRepository2;
            _Mapper = mapper;
            _userService = userService;
        }


        public async Task<Guid> AddAsync(PendingPlanChangeDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // ========================================================
            // AUTHENTICATION
            // ========================================================

            var currentUserId = _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Authenticated user is required.");

            var currentUser =
                await _userService.GetUserByIdAsync(
                    currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "Authenticated user was not found.");

           

            var isPlatformAdmin =
                currentUser.Roles.Any(
                    r => string.Equals(
                        r,
                        "Platform Admin",
                        StringComparison.OrdinalIgnoreCase));

            var isCustomerAdmin =
                currentUser.Roles.Any(
                    r => string.Equals(
                        r,
                        "Customer Admin",
                        StringComparison.OrdinalIgnoreCase));

            if (!isPlatformAdmin && !isCustomerAdmin)
                throw new ForbiddenAccessException(
                    "You are not authorized to request a plan downgrade.");

            // ========================================================
            // VALIDATE SUBSCRIPTION ID
            // ========================================================

            if (dto.SubscriptionId == Guid.Empty)
                throw new ArgumentException(
                    "Subscription ID is required.");

            // ========================================================
            // GET SUBSCRIPTION
            // ========================================================

            var subscription =
                await _SubscriptionRepo
                    .GetTable()
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == dto.SubscriptionId &&
                            x.CurrentState > 0);

            if (subscription == null)
                throw new KeyNotFoundException(
                    "Subscription not found.");

            // ========================================================
            // TENANT ISOLATION
            // ========================================================

            if (!isPlatformAdmin)
            {
                if (!currentUser.OrganizationId.HasValue)
                    throw new UnauthorizedAccessException(
                        "User organization was not found.");

                if (subscription.OrganizationId !=
                    currentUser.OrganizationId.Value)
                {
                    throw new ForbiddenAccessException(
                        "You are not authorized to access this subscription.");
                }
            }

            // ========================================================
            // CURRENT BILLING PERIOD MUST EXIST
            // ========================================================

            if (!subscription.CurrentPeriodEnd.HasValue)
                throw new InvalidOperationException(
                    "The subscription does not have a current billing period end date.");

            // ========================================================
            // VALIDATE TARGET PLAN ID
            // ========================================================

            if (dto.TargetPlanId == Guid.Empty)
                throw new ArgumentException(
                    "Target plan ID is required.");

            // ========================================================
            // TARGET PLAN MUST EXIST
            // ========================================================

            var targetPlan =
                await _PlanRepo
                    .GetTable()
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == dto.TargetPlanId &&
                            x.CurrentState > 0);

            if (targetPlan == null)
                throw new KeyNotFoundException(
                    "Target plan not found.");

            

            dto.EffectiveDate =
                subscription.CurrentPeriodEnd.Value;

            dto.IsApplied = false;

            // ========================================================
            // RECORD THE FUTURE PLAN CHANGE
            // ========================================================

            return await base.AddAsync(dto);
        }
        public async Task<PendingPlanChangeDto?> GetBySubscriptionIdAsync(Guid subscriptionId)
        {
            // ========================================================
            // VALIDATE SUBSCRIPTION ID
            // ========================================================

            if (subscriptionId == Guid.Empty)
                throw new ArgumentException(
                    "Subscription ID is required.");

            // ========================================================
            // AUTHENTICATION
            // ========================================================

            var currentUserId = _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Authenticated user is required.");

            var currentUser =
                await _userService.GetUserByIdAsync(
                    currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "Authenticated user was not found.");

            // ========================================================
            // AUTHORIZATION
            // Platform Admin + Customer Admin can view pending changes.
            // ========================================================

            var isPlatformAdmin =
                currentUser.Roles.Any(
                    r => string.Equals(
                        r,
                        "Platform Admin",
                        StringComparison.OrdinalIgnoreCase));

            var isCustomerAdmin =
                currentUser.Roles.Any(
                    r => string.Equals(
                        r,
                        "Customer Admin",
                        StringComparison.OrdinalIgnoreCase));

            if (!isPlatformAdmin && !isCustomerAdmin)
                throw new ForbiddenAccessException(
                    "You are not authorized to view pending plan changes.");

            // ========================================================
            // GET ACTIVE SUBSCRIPTION
            // ========================================================

            var subscription =
                await _SubscriptionRepo
                    .GetTable()
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == subscriptionId &&
                            x.CurrentState > 0);

            if (subscription == null)
                throw new KeyNotFoundException(
                    "Subscription not found.");

           

            if (!isPlatformAdmin)
            {
                if (!currentUser.OrganizationId.HasValue)
                    throw new UnauthorizedAccessException(
                        "User organization was not found.");

                if (subscription.OrganizationId !=
                    currentUser.OrganizationId.Value)
                {
                    throw new ForbiddenAccessException(
                        "You are not authorized to access this subscription.");
                }
            }

           

            var pendingPlanChange =
                await _PendingPlanChangeRepo
                    .GetTable()
                    .Include(x => x.Subscription)
                        .ThenInclude(x => x.Organization)
                    .Include(x => x.Subscription)
                        .ThenInclude(x => x.Plan)
                    .Include(x => x.Plan)
                    .FirstOrDefaultAsync(
                        x =>
                            x.SubscriptionId == subscriptionId &&
                            x.CurrentState > 0 &&
                            !x.IsApplied);

            // ========================================================
            // NO PENDING PLAN CHANGE
            // ========================================================

            if (pendingPlanChange == null)
                return null;

            // ========================================================
            // MAP RESULT
            // ========================================================

            return _Mapper.Map<PendingPlanChangeDto>(pendingPlanChange);
        }
        public async Task ApplyAsync(Guid subscriptionId)
        {
            // ========================================================
            // VALIDATE SUBSCRIPTION ID
            // ========================================================

            if (subscriptionId == Guid.Empty)
                throw new ArgumentException(
                    "Subscription ID is required.");

            // ========================================================
            // AUTHENTICATION
            // ========================================================

            var currentUserId = _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Authenticated user is required.");

            var currentUser =
                await _userService.GetUserByIdAsync(
                    currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "Authenticated user was not found.");

            // ========================================================
            // AUTHORIZATION
            // ========================================================

            var isPlatformAdmin =
                currentUser.Roles.Any(
                    r => string.Equals(
                        r,
                        "Platform Admin",
                        StringComparison.OrdinalIgnoreCase));

            var isCustomerAdmin =
                currentUser.Roles.Any(
                    r => string.Equals(
                        r,
                        "Customer Admin",
                        StringComparison.OrdinalIgnoreCase));

            if (!isPlatformAdmin && !isCustomerAdmin)
                throw new ForbiddenAccessException(
                    "You are not authorized to apply a pending plan change.");

            // ========================================================
            // GET ACTIVE SUBSCRIPTION
            // ========================================================

            var subscription =
                await _SubscriptionRepo
                    .GetTable()
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == subscriptionId &&
                            x.CurrentState > 0);

            if (subscription == null)
                throw new KeyNotFoundException(
                    "Subscription not found.");

            // ========================================================
            // TENANT ISOLATION
            // ========================================================

            if (!isPlatformAdmin)
            {
                if (!currentUser.OrganizationId.HasValue)
                    throw new UnauthorizedAccessException(
                        "User organization was not found.");

                if (subscription.OrganizationId !=
                    currentUser.OrganizationId.Value)
                {
                    throw new ForbiddenAccessException(
                        "You are not authorized to access this subscription.");
                }
            }

            

            if (subscription.Status != SubscriptionStatus.Active &&
                subscription.Status != SubscriptionStatus.ActiveUntilPeriodEnd)
            {
                throw new InvalidOperationException(
                    "A pending plan downgrade can only be applied to an active subscription.");
            }

            // ========================================================
            // CURRENT BILLING PERIOD MUST EXIST
            // ========================================================

            if (!subscription.CurrentPeriodEnd.HasValue)
                throw new InvalidOperationException(
                    "The subscription does not have a current billing period end date.");

          

            var pendingPlanChange =
                await _PendingPlanChangeRepo
                    .GetTable()
                    .FirstOrDefaultAsync(
                        x =>
                            x.SubscriptionId == subscriptionId &&
                            x.CurrentState > 0 &&
                            !x.IsApplied);

            if (pendingPlanChange == null)
                throw new KeyNotFoundException(
                    "No pending plan change was found for this subscription.");

           

            if (pendingPlanChange.EffectiveDate !=
                subscription.CurrentPeriodEnd.Value)
            {
                throw new InvalidOperationException(
                    "The pending plan change effective date does not match the subscription current period end.");
            }

            // ========================================================
            // IT MUST NOT BE APPLIED BEFORE ITS EFFECTIVE TIME
            // ========================================================

            if (DateTime.UtcNow < pendingPlanChange.EffectiveDate)
                throw new InvalidOperationException(
                    "The pending plan change cannot be applied before its effective date.");

           

            var targetPlan =
                await _PlanRepo
                    .GetTable()
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == pendingPlanChange.TargetPlanId &&
                            x.CurrentState > 0);

            if (targetPlan == null)
                throw new KeyNotFoundException(
                    "The target plan was not found.");

            // ========================================================
            // TARGET PLAN MUST DIFFER FROM CURRENT PLAN
            // ========================================================

            if (pendingPlanChange.TargetPlanId == subscription.PlanId)
                throw new InvalidOperationException(
                    "The target plan is already the current subscription plan.");

          

            subscription.PlanId =
                pendingPlanChange.TargetPlanId;

            pendingPlanChange.IsApplied = true;

            // ========================================================
            // PERSIST BOTH CHANGES
            // ========================================================

            await _SubscriptionRepo.UpdateAsync(
                subscription,
                currentUserId);

            await _PendingPlanChangeRepo.UpdateAsync(
                pendingPlanChange,
                currentUserId);
        }
    }
}