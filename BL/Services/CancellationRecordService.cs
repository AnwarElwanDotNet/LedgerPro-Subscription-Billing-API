
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
    public class CancellationRecordService
        : BaseService<TbCancellationRecord, CancellationRecordDto>,
          ICancellationRecord
    {
        private readonly ITableRepository<TbCancellationRecord> _CancellationRecordRepo;
        private readonly ITableRepository<TbSubscription> _SubscriptionRepo;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public CancellationRecordService(
            ITableRepository<TbCancellationRecord> tableRepository,
            ITableRepository<TbSubscription> tableRepository1,
            IMapper mapper,
            IUserService userService)
            : base(tableRepository, mapper, userService)
        {
            _CancellationRecordRepo = tableRepository;
            _SubscriptionRepo = tableRepository1;
            _mapper = mapper;
            _userService = userService;
        }

        // ============================================================
        // Get Cancellation Record By Id
        // ============================================================
        public async Task<CancellationRecordDto> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(
                    "Cancellation record ID is required.");
            var currentUserId = _userService.GetLoggedInUser();

            var currentUser = await _userService.GetUserByIdAsync(
                currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "Authenticated user was not found.");

            var isPlatformAdmin =
                currentUser.Roles.Contains("Platform Admin");

            var query = _CancellationRecordRepo
                .GetTable()
                .Include(x => x.Organization)
                .Include(x => x.Subscription)
                    .ThenInclude(x => x.Plan)
                .Where(x => x.Id == id);

            // ========================================================
            // Tenant Isolation
            // ========================================================
            if (!isPlatformAdmin)
            {
                if (!currentUser.OrganizationId.HasValue)
                    throw new ForbiddenAccessException(
   "Authenticated user is not associated with an organization.");
             

                query = query.Where(
                    x => x.OrganizationId == currentUser.OrganizationId.Value);
            }

            var record = await query.FirstOrDefaultAsync();

            if (record == null)
                throw new KeyNotFoundException(
                    "Cancellation record was not found.");

            return _mapper.Map<CancellationRecordDto>(record);
        }

        // ============================================================
        // Get Cancellation Records By Subscription
        // ============================================================
        public async Task<IEnumerable<CancellationRecordDto>>
            GetBySubscriptionIdAsync(Guid subscriptionId)
        {
            if (subscriptionId == Guid.Empty)
                throw new ArgumentException(
                    "Subscription ID is required.");
            var currentUserId = _userService.GetLoggedInUser();

            var currentUser = await _userService.GetUserByIdAsync(
                currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "Authenticated user was not found.");

            var isPlatformAdmin =
                currentUser.Roles.Contains("Platform Admin");

            // ========================================================
            // Validate Subscription
            // ========================================================
            var subscription = await _SubscriptionRepo
                .GetTable()
                .FirstOrDefaultAsync(x => x.Id == subscriptionId);

            if (subscription == null)
                throw new KeyNotFoundException(
                    "Subscription was not found.");

            // ========================================================
            // Tenant Isolation
            // ========================================================
            if (!isPlatformAdmin)
            {
                if (!currentUser.OrganizationId.HasValue)
                    throw new UnauthorizedAccessException(
                        "Authenticated user is not associated with an organization.");

                if (subscription.OrganizationId !=
                    currentUser.OrganizationId.Value)
                {
                    throw new ForbiddenAccessException(
     "You are not authorized to access this subscription.");
                }
            }

            // ========================================================
            // Get Historical Cancellation Records
            // ========================================================
            var query = _CancellationRecordRepo
                .GetTable()
                .Include(x => x.Organization)
                .Include(x => x.Subscription)
                    .ThenInclude(x => x.Plan)
                .Where(x => x.SubscriptionId == subscriptionId);

            // Extra tenant protection
            if (!isPlatformAdmin)
            {
                query = query.Where(
                    x => x.OrganizationId ==
                         currentUser.OrganizationId!.Value);
            }

            var records = await query
                .OrderByDescending(x => x.PerformedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CancellationRecordDto>>(records);
        }

       
        public async Task<Guid> AddAsync(
            CancellationRecordDto dto,
            bool refundInitiated)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var currentUserId = _userService.GetLoggedInUser();

            var currentUser = await _userService.GetUserByIdAsync(
                currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "Authenticated user was not found.");

            var isPlatformAdmin =
                currentUser.Roles.Contains("Platform Admin");

            var isCustomerAdmin =
                currentUser.Roles.Contains("Customer Admin");

           
            if (!isPlatformAdmin && !isCustomerAdmin)
            {
                throw new ForbiddenAccessException(
                    "You are not authorized to cancel this subscription.");
            }

            // ========================================================
            // Validate Cancellation Type
            // ========================================================
            if (!Enum.IsDefined(typeof(CancellationType), dto.Type))
            {
                throw new ArgumentException(
                    "Invalid cancellation type.");
            }


            // ========================================================
            // Validate Subscription
            // ========================================================
            var subscription = await _SubscriptionRepo
                .GetTable()
                .FirstOrDefaultAsync(x => x.Id == dto.SubscriptionId);

            if (subscription == null)
                throw new KeyNotFoundException(
                    "Subscription was not found.");

           
            if (!isPlatformAdmin)
            {
                if (!currentUser.OrganizationId.HasValue)
                    throw new UnauthorizedAccessException(
                        "Authenticated user is not associated with an organization.");

                if (subscription.OrganizationId !=
     currentUser.OrganizationId.Value)
                {
                    throw new ForbiddenAccessException(
                        "You are not authorized to cancel this subscription.");
                }
            }

           
            if (subscription.Status == SubscriptionStatus.PendingPayment)
            {
                throw new InvalidOperationException(
                    "A pending-payment subscription cannot be cancelled.");
            }

            if (subscription.Status == SubscriptionStatus.Cancelled)
            {
                throw new InvalidOperationException(
                    "This subscription has already been cancelled.");
            }

            if (subscription.Status == SubscriptionStatus.Expired)
            {
                throw new InvalidOperationException(
                    "An expired subscription cannot be cancelled.");
            }

            if (subscription.Status == SubscriptionStatus.ActiveUntilPeriodEnd)
            {
                throw new InvalidOperationException(
                    "A cancellation request already exists for this subscription.");
            }

            
            if (dto.Type == CancellationType.EndOfPeriod)
            {
                if (!subscription.CurrentPeriodEnd.HasValue)
                {
                    throw new InvalidOperationException(
                        "The subscription does not have a current billing period end date.");
                }

                if (dto.AccessEndedImmediately)
                {
                    throw new InvalidOperationException(
                        "End-of-period cancellation cannot end access immediately.");
                }
            }

            // ========================================================
            // Validate Immediate Cancellation
            // ========================================================
            if (dto.Type == CancellationType.Immediate &&
     !isPlatformAdmin)
            {
                throw new ForbiddenAccessException(
                    "Immediate cancellation is restricted to authorized administrative users.");
            }

         
            var cancellationTime = DateTime.UtcNow;

            
            DateTime effectiveDate;

            if (dto.Type == CancellationType.EndOfPeriod)
            {
                effectiveDate = subscription.CurrentPeriodEnd!.Value;
            }
            else
            {
                effectiveDate = cancellationTime;
            }

           
            var organizationId = subscription.OrganizationId;

            // ========================================================
            // Map DTO -> Entity
            // ========================================================
            var entity = _mapper.Map<TbCancellationRecord>(dto);

            // ========================================================
            // Server-Controlled Values
            // ========================================================
            entity.Id = Guid.NewGuid();

            entity.OrganizationId = organizationId;

            entity.SubscriptionId = subscription.Id;

            entity.PerformedBy = currentUserId;

            entity.PerformedAt = cancellationTime;

            entity.EffectiveDate = effectiveDate;

         
            entity.RefundInitiated = refundInitiated;

         
            await _CancellationRecordRepo.AddAsync(
                entity,
                currentUserId);

            return entity.Id;
        }
    }
}

