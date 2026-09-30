using AutoMapper;
using BL.Contract;
using BL.Dtos;
using BL.Exceptions;
using DAL.Contracts;
using DAL.UserModels;
using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BL.Services
{
    public class UserSeatService : BaseService<TbUserSeat, UserSeatDto>, IUserSeat
    {
        private readonly ITableRepository<TbUserSeat> _UserSeatRepo;
        private readonly ITableRepository<TbSubscription> _SubscriptionRepo;
        private readonly ITableRepository<TbBillingPeriod> _BillingPeriodRepo;
        private readonly ITableRepository<TbPlan> _PlanRepo;
        private readonly ITableRepository<TbInvoice> _InvoiceRepo;
        private readonly ITableRepository<TbPayment> _PaymentRepo;
        private readonly IViewRepository<ApplicationUser> _UserRepo;

        private readonly IAuditLog _AuditLog;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserSeatService(
            ITableRepository<TbUserSeat> tableRepository,
            ITableRepository<TbSubscription> subscriptionRepository,
                ITableRepository<TbBillingPeriod> billingPeriodRepository,

            ITableRepository<TbPlan> planRepository,
            ITableRepository<TbInvoice> invoiceRepository,
            ITableRepository<TbPayment> paymentRepository,
            IViewRepository<ApplicationUser> userRepository,
            IAuditLog auditLog,
            IMapper mapper,
            IUserService userService,
            UserManager<ApplicationUser> userManager)
            : base(tableRepository, mapper, userService)
        {
            _UserSeatRepo = tableRepository;
            _SubscriptionRepo = subscriptionRepository;
            _BillingPeriodRepo = billingPeriodRepository;
            _PlanRepo = planRepository;
            _InvoiceRepo = invoiceRepository;
            _PaymentRepo = paymentRepository;
            _UserRepo = userRepository;

            _AuditLog = auditLog;
            _mapper = mapper;
            _userService = userService;
            _userManager = userManager;
        }

        public async Task<PagedResult<UserSeatDto>> GetAllAsync(
            int pageNumber,
            int pageSize)
        {
           

            if (pageNumber <= 0)
                throw new ArgumentException(
                    "Page number must be greater than zero.");

            if (pageSize <= 0)
                throw new ArgumentException(
                    "Page size must be greater than zero.");


            var loggedInUserId = _userService.GetLoggedInUser();

            if (loggedInUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Logged-in user could not be identified.");

            var loggedInUser = await _UserRepo.GetByIdAsync(loggedInUserId);

            if (loggedInUser == null)
                throw new UnauthorizedAccessException(
                    "Logged-in user could not be found.");

         

            var roles = await _userManager.GetRolesAsync(loggedInUser);

            var isPlatformAdmin = roles.Contains("Platform Admin");

           

            IQueryable<TbUserSeat> seatQuery = _UserSeatRepo
                .GetTable()
                .Where(x =>
                    x.CurrentState > 0 &&
                    x.Status == UserSeatStatus.Active);

           

            if (!isPlatformAdmin)
            {
                if (loggedInUser.OrganizationId == null ||
                    loggedInUser.OrganizationId == Guid.Empty)
                {
                    throw new ForbiddenAccessException(
                        "User is not associated with an organization.");
                }

                seatQuery = seatQuery.Where(
                    x => x.OrganizationId == loggedInUser.OrganizationId.Value);
            }

            

            var resultQuery =
                from seat in seatQuery

                join user in _UserRepo.GetTable()
                    on seat.UserId equals user.Id

                join subscription in _SubscriptionRepo.GetTable()
                    on seat.SubscriptionId equals subscription.Id

                join plan in _PlanRepo.GetTable()
                    on subscription.PlanId equals plan.Id

                where
                    user.CurrentState > 0 &&
                    subscription.CurrentState > 0 &&
                    user.OrganizationId == seat.OrganizationId &&
                    subscription.OrganizationId == seat.OrganizationId

                select new UserSeatDto
                {
                   

                    Id = seat.Id,

                 

                    UserId = seat.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,

                  

                    OrganizationId = seat.OrganizationId,

                    OrganizationName =
                        subscription.Organization != null
                            ? subscription.Organization.OrganizationName
                            : null,

                   

                    SubscriptionId = seat.SubscriptionId,
                    SubscriptionStatus = subscription.Status,

                   

                    PlanName = plan.PlanName,
                    Description = plan.Description,
                    Currency = plan.Currency,

                   

                    Status = seat.Status
                };

          

            var totalCount = await resultQuery.CountAsync();

         

            var result = await resultQuery
                .OrderBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

           

            return new PagedResult<UserSeatDto>
            {
                Items = result,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        public async Task<UserSeatDto?> GetByIdAsync(Guid id)
        {
           

            if (id == Guid.Empty)
                throw new ArgumentException(
                    "User seat ID is required.");

         

            var loggedInUserId = _userService.GetLoggedInUser();

            if (loggedInUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Logged-in user could not be identified.");

            var loggedInUser = await _UserRepo.GetByIdAsync(loggedInUserId);

            if (loggedInUser == null)
                throw new UnauthorizedAccessException(
                    "Logged-in user could not be found.");

         

            var roles = await _userManager.GetRolesAsync(loggedInUser);

            var isPlatformAdmin = roles.Contains("Platform Admin");

          

            if (!isPlatformAdmin)
            {
                if (loggedInUser.OrganizationId == null ||
                    loggedInUser.OrganizationId == Guid.Empty)
                {
                    throw new ForbiddenAccessException(
                        "User is not associated with an organization.");
                }
            }

          

            var seatQuery =
                from userSeat in _UserSeatRepo.GetTable()

                join user in _UserRepo.GetTable()
                    on userSeat.UserId equals user.Id

                join subscription in _SubscriptionRepo.GetTable()
                    on userSeat.SubscriptionId equals subscription.Id

                join plan in _PlanRepo.GetTable()
                    on subscription.PlanId equals plan.Id

                where
                    userSeat.Id == id &&
                    userSeat.CurrentState > 0 &&

                   

                    user.OrganizationId == userSeat.OrganizationId &&
                    subscription.OrganizationId == userSeat.OrganizationId

                select new UserSeatDto
                {
                   

                    Id = userSeat.Id,

                    UserId = userSeat.UserId,

                

                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,

                   

                    OrganizationId = userSeat.OrganizationId,

                    OrganizationName =
                        subscription.Organization != null
                            ? subscription.Organization.OrganizationName
                            : null,

                   

                    SubscriptionId = userSeat.SubscriptionId,
                    SubscriptionStatus = subscription.Status,

                  

                    PlanName = plan.PlanName,
                    Description = plan.Description,
                    Currency = plan.Currency,

                  

                    Status = userSeat.Status
                };

          

            if (!isPlatformAdmin)
            {
                seatQuery = seatQuery.Where(
                    x => x.OrganizationId == loggedInUser.OrganizationId!.Value);
            }

           

            var seat = await seatQuery.FirstOrDefaultAsync();

           

            return seat;
        }
        public async Task<UserSeatDto> AddUserSeatAsync(
            Guid organizationId,
            Guid subscriptionId,
            Guid userId)
        {
           

            if (organizationId == Guid.Empty)
                throw new ArgumentException(
                    "Organization ID is required.");

            if (subscriptionId == Guid.Empty)
                throw new ArgumentException(
                    "Subscription ID is required.");

            if (userId == Guid.Empty)
                throw new ArgumentException(
                    "User ID is required.");

        

            var loggedInUserId = _userService.GetLoggedInUser();

            if (loggedInUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Logged-in user could not be identified.");

            var loggedInUser = await _UserRepo.GetByIdAsync(loggedInUserId);

            if (loggedInUser == null)
                throw new UnauthorizedAccessException(
                    "Logged-in user could not be found.");

          

            var roles = await _userManager.GetRolesAsync(loggedInUser);

            var isPlatformAdmin = roles.Contains("Platform Admin");
            var isCustomerAdmin = roles.Contains("Customer Admin");

            if (!isPlatformAdmin && !isCustomerAdmin)
            {
                throw new ForbiddenAccessException(
                    "Only Platform Admin or Customer Admin can manage user seats.");
            }

          

            if (!isPlatformAdmin)
            {
                if (loggedInUser.OrganizationId == null ||
                    loggedInUser.OrganizationId == Guid.Empty)
                {
                    throw new ForbiddenAccessException(
                        "User is not associated with an organization.");
                }

                if (loggedInUser.OrganizationId.Value != organizationId)
                {
                    throw new ForbiddenAccessException(
                        "You are not authorized to manage seats for this organization.");
                }
            }

           

            UserSeatDto? addedSeat = null;

            await _UserSeatRepo.ExecuteInTransactionAsync(
                async () =>
                {
                    var now = DateTime.UtcNow;

                  

                    var targetUser = await _UserRepo.GetByIdAsync(userId);

                    if (targetUser == null)
                    {
                        throw new KeyNotFoundException(
                            "User could not be found.");
                    }

                    if (targetUser.CurrentState <= 0)
                    {
                        throw new InvalidOperationException(
                            "The user is not active.");
                    }

                  

                    if (targetUser.OrganizationId == null ||
                        targetUser.OrganizationId == Guid.Empty)
                    {
                        throw new ForbiddenAccessException(
                            "User is not associated with an organization.");
                    }

                    if (targetUser.OrganizationId.Value != organizationId)
                    {
                        throw new ForbiddenAccessException(
                            "User does not belong to the specified organization.");
                    }

                   

                    var subscription = await _SubscriptionRepo.GetByIdAsync(
                        subscriptionId);

                    if (subscription == null ||
                        subscription.CurrentState <= 0)
                    {
                        throw new KeyNotFoundException(
                            "Subscription could not be found.");
                    }

                    if (subscription.OrganizationId != organizationId)
                    {
                        throw new ForbiddenAccessException(
                            "Subscription does not belong to the specified organization.");
                    }

                   

                    if (targetUser.OrganizationId.Value != subscription.OrganizationId)
                    {
                        throw new InvalidOperationException(
                            "The user and subscription do not belong to the same organization.");
                    }

                   

                    if (!subscription.CurrentPeriodStart.HasValue ||
                        !subscription.CurrentPeriodEnd.HasValue)
                    {
                        throw new InvalidOperationException(
                            "Subscription does not have a valid billing period.");
                    }

                    if (now < subscription.CurrentPeriodStart.Value ||
                        now >= subscription.CurrentPeriodEnd.Value)
                    {
                        throw new InvalidOperationException(
                            "The subscription does not have a valid current billing period.");
                    }

                   

                    var currentBillingPeriod = await _BillingPeriodRepo.GetFirstOrDefaultAsync(
                        x =>
                            x.SubscriptionId == subscriptionId &&
                            x.Start == subscription.CurrentPeriodStart.Value &&
                            x.End == subscription.CurrentPeriodEnd.Value &&
                            x.CurrentState > 0 &&
                            x.Status == BillingPeriodStatus.Active);

                    if (currentBillingPeriod == null)
                    {
                        throw new InvalidOperationException(
                            "The subscription does not have a valid current billing period record.");
                    }

                   

                    if (subscription.Status != SubscriptionStatus.Active &&
                        subscription.Status != SubscriptionStatus.ActiveUntilPeriodEnd)
                    {
                        throw new InvalidOperationException(
                            "The subscription does not currently provide paid entitlement.");
                    }

                   

                    var plan = await _PlanRepo.GetByIdAsync(
                        subscription.PlanId);

                    if (plan == null)
                    {
                        throw new InvalidOperationException(
                            "Subscription plan could not be found.");
                    }

                    if (plan.MaxUsers <= 0)
                    {
                        throw new InvalidOperationException(
                            "The subscription plan does not have a valid user limit.");
                    }

                   

                    var successfulPaymentExists = await (
       from payment in _PaymentRepo.GetTable()

       join invoice in _InvoiceRepo.GetTable()
           on payment.InvoiceId equals invoice.Id

       where
           // -----------------------------------------------------------------
           // Ownership
           // -----------------------------------------------------------------

           invoice.SubscriptionId == subscriptionId &&
           invoice.OrganizationId == organizationId &&

           // -----------------------------------------------------------------
           // Exact current BillingPeriod relationship
           // -----------------------------------------------------------------

           invoice.BillingPeriodId == currentBillingPeriod.Id &&

           // -----------------------------------------------------------------
           // Successful financial state
           // -----------------------------------------------------------------

           payment.Status == PaymentStatus.Succeeded &&
           invoice.Status == InvoiceStatus.Paid &&

           // -----------------------------------------------------------------
           // Active records
           // -----------------------------------------------------------------

           payment.CurrentState > 0 &&
           invoice.CurrentState > 0 &&

           // -----------------------------------------------------------------
           // Financial consistency
           // -----------------------------------------------------------------

           payment.Amount == invoice.Amount &&
           payment.Currency == invoice.Currency

       select payment.Id
   ).AnyAsync();

                    if (!successfulPaymentExists)
                    {
                        throw new InvalidOperationException(
                            "The subscription does not have a successful payment for the current billing period.");
                    }

                    // =========================================================================
                    // 5.8 Prevent duplicate active seat
                    // =========================================================================

                    var existingSeat = await _UserSeatRepo
                        .GetFirstOrDefaultAsync(x =>
                            x.UserId == userId &&
                            x.OrganizationId == organizationId &&
                            x.SubscriptionId == subscriptionId &&
                            x.Status == UserSeatStatus.Active &&
                            x.CurrentState > 0);

                    if (existingSeat != null)
                    {
                        throw new InvalidOperationException(
                            "The user already has an active seat for this subscription.");
                    }

                    // =========================================================================
                    // 5.9 Count ONLY active users consuming seats
                    //
                    // Historical/inactive seats do not consume capacity.
                    // Inactive users do not consume capacity.
                    // =========================================================================

                    var activeSeatCount = await (
                        from seat in _UserSeatRepo.GetTable()

                        join user in _UserRepo.GetTable()
                            on seat.UserId equals user.Id

                        where
                            seat.OrganizationId == organizationId &&
                            seat.SubscriptionId == subscriptionId &&
                            seat.Status == UserSeatStatus.Active &&
                            seat.CurrentState > 0 &&
                            user.CurrentState > 0

                        select seat.Id
                    ).CountAsync();

                    // =========================================================================
                    // 5.10 Enforce plan user limit
                    // =========================================================================

                    if (activeSeatCount >= plan.MaxUsers)
                    {
                        throw new InvalidOperationException(
                            "The subscription has reached the maximum number of active users allowed by the plan.");
                    }

                    // =========================================================================
                    // 5.11 Create active UserSeat
                    // =========================================================================

                    var entity = new TbUserSeat
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        OrganizationId = organizationId,
                        SubscriptionId = subscriptionId,
                        Status = UserSeatStatus.Active,
                        CurrentState = 1
                    };

                    await _UserSeatRepo.AddAsync(
                        entity,
                        loggedInUserId);

                    // =========================================================================
                    // 5.12 Audit
                    // =========================================================================

                    var auditDto = new AuditLogDto
                    {
                        UserId = loggedInUserId,
                        Action = "Add User Seat",
                        EntityType = EntityType.UserSeat,
                        EntityId = entity.Id,
                        CorrelationId = Guid.NewGuid(),
                        Metadata =
                            $"UserId={userId};OrganizationId={organizationId};SubscriptionId={subscriptionId}"
                    };

                    await _AuditLog.AddAsync(auditDto);

                    // =========================================================================
                    // 5.13 Build returned DTO
                    // =========================================================================

                    addedSeat = await (
                        from seat in _UserSeatRepo.GetTable()

                        join user in _UserRepo.GetTable()
                            on seat.UserId equals user.Id

                        join currentSubscription in _SubscriptionRepo.GetTable()
                            on seat.SubscriptionId equals currentSubscription.Id

                        join currentPlan in _PlanRepo.GetTable()
                            on currentSubscription.PlanId equals currentPlan.Id

                        where
                            seat.Id == entity.Id &&
                            seat.CurrentState > 0

                        select new UserSeatDto
                        {
                            Id = seat.Id,

                            UserId = seat.UserId,

                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            UserName = user.UserName,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,

                            OrganizationId = seat.OrganizationId,

                            OrganizationName =
                                currentSubscription.Organization != null
                                    ? currentSubscription.Organization.OrganizationName
                                    : null,

                            SubscriptionId = seat.SubscriptionId,

                            SubscriptionStatus =
                                currentSubscription.Status,

                            PlanName = currentPlan.PlanName,
                            Description = currentPlan.Description,
                            Currency = currentPlan.Currency,

                            Status = seat.Status
                        }
                    ).FirstOrDefaultAsync();
                },
                IsolationLevel.Serializable);

            // =====================================================================================
            // 6. Ensure creation succeeded
            // =====================================================================================

            if (addedSeat == null)
            {
                throw new InvalidOperationException(
                    "The user seat could not be created.");
            }

            return addedSeat;
        }
        public async Task<UserSeatDto> RemoveUserSeatAsync(
        Guid userSeatId)
        {
            // =====================================================================================
            // 1. Validate ID
            // =====================================================================================

            if (userSeatId == Guid.Empty)
                throw new ArgumentException(
                    "User seat ID is required.");

            // =====================================================================================
            // 2. Identify logged-in user
            // =====================================================================================

            var loggedInUserId = _userService.GetLoggedInUser();

            if (loggedInUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Logged-in user could not be identified.");

            var loggedInUser = await _UserRepo.GetByIdAsync(
                loggedInUserId);

            if (loggedInUser == null)
                throw new UnauthorizedAccessException(
                    "Logged-in user could not be found.");

            // =====================================================================================
            // 3. Authorization
            //
            // Only Platform Admin and Customer Admin can remove user seats.
            // Customer User cannot manage seats.
            // =====================================================================================

            var roles = await _userManager.GetRolesAsync(
                loggedInUser);

            var isPlatformAdmin = roles.Contains("Platform Admin");
            var isCustomerAdmin = roles.Contains("Customer Admin");

            if (!isPlatformAdmin && !isCustomerAdmin)
            {
                throw new ForbiddenAccessException(
                    "Only Platform Admin or Customer Admin can manage user seats.");
            }

            // =====================================================================================
            // 4. Get seat
            // =====================================================================================

            var seat = await _UserSeatRepo.GetByIdAsync(
                userSeatId);

            if (seat == null ||
                seat.CurrentState <= 0)
            {
                throw new KeyNotFoundException(
                    "User seat could not be found.");
            }

            // =====================================================================================
            // 5. Server-side tenant isolation
            //
            // Platform Admin:
            //     Can manage seats across organizations.
            //
            // Customer Admin:
            //     Can manage seats only inside their own organization.
            // =====================================================================================

            if (!isPlatformAdmin)
            {
                if (loggedInUser.OrganizationId == null ||
                    loggedInUser.OrganizationId == Guid.Empty)
                {
                    throw new ForbiddenAccessException(
                        "User is not associated with an organization.");
                }

                if (loggedInUser.OrganizationId.Value !=
                    seat.OrganizationId)
                {
                    throw new ForbiddenAccessException(
                        "You are not authorized to remove this user seat.");
                }
            }

            // =====================================================================================
            // 6. Seat must currently be active
            //
            // An inactive seat no longer consumes capacity and cannot be removed again.
            // Historical seat records remain preserved.
            // =====================================================================================

            if (seat.Status != UserSeatStatus.Active)
            {
                throw new InvalidOperationException(
                    "The user seat is already inactive.");
            }

            // =====================================================================================
            // 7. Transaction + concurrency protection
            //
            // The historical UserSeat record is preserved.
            // Only its status is changed from Active -> Inactive.
            //
            // All mutable authorization/ownership conditions are re-validated inside
            // the transaction before the update is committed.
            // =====================================================================================

            UserSeatDto? removedSeat = null;

            await _UserSeatRepo.ExecuteInTransactionAsync(
                async () =>
                {
                    // =========================================================================
                    // 7.1 Re-read seat inside transaction
                    // =========================================================================

                    var currentSeat = await _UserSeatRepo.GetByIdAsync(
                        userSeatId);

                    if (currentSeat == null ||
                        currentSeat.CurrentState <= 0)
                    {
                        throw new KeyNotFoundException(
                            "User seat could not be found.");
                    }

                    // =========================================================================
                    // 7.2 Prevent duplicate/concurrent removal
                    // =========================================================================

                    if (currentSeat.Status != UserSeatStatus.Active)
                    {
                        throw new InvalidOperationException(
                            "The user seat is already inactive.");
                    }

                    // =========================================================================
                    // 7.3 Re-validate tenant ownership inside transaction
                    //
                    // Customer Admin must still belong to the same organization
                    // represented by the UserSeat.
                    // =========================================================================

                    if (!isPlatformAdmin)
                    {
                        if (loggedInUser.OrganizationId == null ||
                            loggedInUser.OrganizationId == Guid.Empty)
                        {
                            throw new ForbiddenAccessException(
                                "User is not associated with an organization.");
                        }

                        if (loggedInUser.OrganizationId.Value !=
                            currentSeat.OrganizationId)
                        {
                            throw new ForbiddenAccessException(
                                "You are not authorized to remove this user seat.");
                        }
                    }

                    // =========================================================================
                    // 7.4 Validate UserSeat organization consistency
                    //
                    // The seat must reference a user belonging to the same organization.
                    // This is a server-side data integrity / tenant isolation check.
                    // =========================================================================

                    var targetUser = await _UserRepo.GetByIdAsync(
                        currentSeat.UserId);

                    if (targetUser == null)
                    {
                        throw new InvalidOperationException(
                            "The user associated with this user seat could not be found.");
                    }

                    if (targetUser.OrganizationId == null ||
                        targetUser.OrganizationId == Guid.Empty ||
                        targetUser.OrganizationId.Value != currentSeat.OrganizationId)
                    {
                        throw new InvalidOperationException(
                            "The user seat is not consistent with the user's organization.");
                    }

                    // =========================================================================
                    // 7.5 Validate UserSeat subscription organization consistency
                    //
                    // Historical subscriptions remain traceable even if no longer active.
                    // Therefore, we DO NOT require subscription.Status == Active.
                    // We only validate ownership consistency.
                    // =========================================================================

                    var subscription = await _SubscriptionRepo.GetByIdAsync(
                        currentSeat.SubscriptionId);

                    if (subscription == null)
                    {
                        throw new InvalidOperationException(
                            "The subscription associated with this user seat could not be found.");
                    }

                    if (subscription.OrganizationId != currentSeat.OrganizationId)
                    {
                        throw new InvalidOperationException(
                            "The user seat is not consistent with the subscription's organization.");
                    }

                    // =========================================================================
                    // 7.6 Deactivate the seat
                    //
                    // DO NOT delete the record.
                    //
                    // Historical record remains available.
                    // Status = Inactive means the seat no longer consumes capacity.
                    // =========================================================================

                    currentSeat.Status = UserSeatStatus.Inactive;

                    await _UserSeatRepo.UpdateAsync(
                        currentSeat,
                        loggedInUserId);

                    // =========================================================================
                    // 7.7 Audit
                    //
                    // Removing/deactivating a user seat is a sensitive operation
                    // and must be auditable.
                    // =========================================================================

                    var auditDto = new AuditLogDto
                    {
                        UserId = loggedInUserId,
                        Action = "Remove User Seat",
                        EntityType = EntityType.UserSeat,
                        EntityId = currentSeat.Id,
                        CorrelationId = Guid.NewGuid(),
                        Metadata =
                            $"UserId={currentSeat.UserId};" +
                            $"OrganizationId={currentSeat.OrganizationId};" +
                            $"SubscriptionId={currentSeat.SubscriptionId}"
                    };

                    await _AuditLog.AddAsync(
                        auditDto);

                    // =========================================================================
                    // 7.8 Build returned DTO
                    //
                    // Return the preserved historical seat with Status = Inactive.
                    // =========================================================================

                     removedSeat = await (
     from userSeat in _UserSeatRepo.GetTable()

     join user in _UserRepo.GetTable()
         on userSeat.UserId equals user.Id

     join seatSubscription in _SubscriptionRepo.GetTable()
         on userSeat.SubscriptionId equals seatSubscription.Id

     join plan in _PlanRepo.GetTable()
         on seatSubscription.PlanId equals plan.Id

     where
         userSeat.Id == currentSeat.Id &&
         userSeat.CurrentState > 0 &&
         user.OrganizationId == userSeat.OrganizationId &&
         seatSubscription.OrganizationId == userSeat.OrganizationId

     select new UserSeatDto
     {
         Id = userSeat.Id,

         UserId = userSeat.UserId,

         FirstName = user.FirstName,
         LastName = user.LastName,
         UserName = user.UserName,
         Email = user.Email,
         PhoneNumber = user.PhoneNumber,

         OrganizationId = userSeat.OrganizationId,

         OrganizationName =
             seatSubscription.Organization != null
                 ? seatSubscription.Organization.OrganizationName
                 : null,

         SubscriptionId = userSeat.SubscriptionId,

         SubscriptionStatus =
             seatSubscription.Status,

         PlanName = plan.PlanName,
         Description = plan.Description,
         Currency = plan.Currency,

         Status = userSeat.Status
     }
 ).FirstOrDefaultAsync();
                },
                IsolationLevel.Serializable);

            // =====================================================================================
            // 8. Ensure operation succeeded
            // =====================================================================================

            if (removedSeat == null)
            {
                throw new InvalidOperationException(
                    "The user seat could not be removed.");
            }

            return removedSeat;
        }
    }
}