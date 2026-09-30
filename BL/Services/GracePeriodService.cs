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
    public class GracePeriodService
        : BaseService<TbGracePeriod, GracePeriodDto>,
          IGracePeriod
    {
        private readonly ITableRepository<TbGracePeriod> _GracePeriodRepo;
        private readonly ITableRepository<TbSubscription> _SubscriptionRepo;
        private readonly ITableRepository<TbInvoice> _InvoiceRepo;
        private readonly ITableRepository<TbPaymentAttempt> _PaymentAttemptRepo;
        private readonly ITableRepository<TbSystemConfig> _SystemConfigRepo;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GracePeriodService(
            ITableRepository<TbGracePeriod> tableRepository,
            ITableRepository<TbSubscription> tableRepository1,
            ITableRepository<TbInvoice> invoiceRepository,
            ITableRepository<TbPaymentAttempt> paymentAttemptRepository,
            ITableRepository<TbSystemConfig> systemConfigRepository,
            IMapper mapper,
            IUserService userService)
            : base(tableRepository, mapper, userService)
        {
            _GracePeriodRepo = tableRepository;
            _SubscriptionRepo = tableRepository1;
            _InvoiceRepo = invoiceRepository;
            _PaymentAttemptRepo = paymentAttemptRepository;
            _SystemConfigRepo = systemConfigRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<GracePeriodDto> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(
                    "Grace period ID is required.");
            var currentUserId = _userService.GetLoggedInUser();

            var currentUser = await _userService.GetUserByIdAsync(
                currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "Authenticated user was not found.");

            var isPlatformAdmin =
                currentUser.Roles.Contains("Platform Admin");

            var query = _GracePeriodRepo
                .GetTable()
                .Include(x => x.Subscription)
                    .ThenInclude(x => x.Organization)
                .Include(x => x.Subscription)
                    .ThenInclude(x => x.Plan)
                .Include(x => x.Invoice)
                .Where(x => x.Id == id);

            if (!isPlatformAdmin)
            {
                if (!currentUser.OrganizationId.HasValue)
                    throw new ForbiddenAccessException(
                        "Authenticated user is not associated with an organization.");

                query = query.Where(
                    x => x.Subscription.OrganizationId ==
                         currentUser.OrganizationId.Value);
            }

            var gracePeriod = await query.FirstOrDefaultAsync();

            if (gracePeriod == null)
                throw new KeyNotFoundException(
                    "Grace period was not found.");

            return _mapper.Map<GracePeriodDto>(gracePeriod);
        }

        public async Task<IEnumerable<GracePeriodDto>>
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

            var subscription = await _SubscriptionRepo
                .GetTable()
                .FirstOrDefaultAsync(
                    x => x.Id == subscriptionId);

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
     "You are not authorized to access this subscription.");
                }
            }

            var query = _GracePeriodRepo
                .GetTable()
                .Include(x => x.Subscription)
                    .ThenInclude(x => x.Organization)
                .Include(x => x.Subscription)
                    .ThenInclude(x => x.Plan)
                .Include(x => x.Invoice)
                .Where(x => x.SubscriptionId == subscriptionId);

            if (!isPlatformAdmin)
            {
                query = query.Where(
                    x => x.Subscription.OrganizationId ==
                         currentUser.OrganizationId.Value);
            }

            var gracePeriods = await query
                .OrderByDescending(x => x.StartDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<GracePeriodDto>>(
                gracePeriods);
        }

        public async Task<Guid> AddAsync(GracePeriodDto dto)
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
                throw new ForbiddenAccessException(
                    "You are not authorized to create a grace period.");

            if (dto.SubscriptionId == Guid.Empty)
                throw new ArgumentException(
                    "Subscription ID is required.",
                    nameof(dto.SubscriptionId));

            if (dto.InvoiceId == Guid.Empty)
                throw new ArgumentException(
                    "Invoice ID is required.",
                    nameof(dto.InvoiceId));

            var subscription = await _SubscriptionRepo
                .GetTable()
                .FirstOrDefaultAsync(
                    x => x.Id == dto.SubscriptionId);

            if (subscription == null)
                throw new KeyNotFoundException(
                    "Subscription was not found.");

            if (!isPlatformAdmin)
            {
                if (!currentUser.OrganizationId.HasValue)
                    throw new ForbiddenAccessException(
                        "Authenticated user is not associated with an organization.");

                if (subscription.OrganizationId !=
                    currentUser.OrganizationId.Value)
                {
                    throw new ForbiddenAccessException(
     "You are not authorized to access this subscription.");
                }
            }

            
            if (subscription.Status != SubscriptionStatus.PastDue)
                throw new InvalidOperationException(
                    "A grace period can only be created for a past-due subscription.");

            var invoice = await _InvoiceRepo
                .GetTable()
                .FirstOrDefaultAsync(
                    x => x.Id == dto.InvoiceId);

            if (invoice == null)
                throw new KeyNotFoundException(
                    "Invoice was not found.");

            if (invoice.SubscriptionId != subscription.Id)
                throw new InvalidOperationException(
                    "The invoice does not belong to the specified subscription.");

            if (invoice.OrganizationId != subscription.OrganizationId)
                throw new InvalidOperationException(
                    "The invoice does not belong to the specified organization.");

            
            if (invoice.InvoiceType != InvoiceType.Renewal)
                throw new InvalidOperationException(
                    "A grace period can only be created for a renewal invoice.");

            if (invoice.Status != InvoiceStatus.PaymentFailed)
                throw new InvalidOperationException(
                    "A grace period can only be created for a payment-failed invoice.");

            var systemConfig = await _SystemConfigRepo
                .GetTable()
                .FirstOrDefaultAsync();

            if (systemConfig == null)
                throw new InvalidOperationException(
                    "System billing configuration was not found.");

            if (systemConfig.GracePeriodDays <= 0)
                throw new InvalidOperationException(
                    "Grace period duration must be greater than zero.");

            if (systemConfig.RetryDay1 < 0 ||
                systemConfig.RetryDay2 <= systemConfig.RetryDay1 ||
                systemConfig.RetryDay3 <= systemConfig.RetryDay2)
            {
                throw new InvalidOperationException(
                    "The configured retry schedule is invalid.");
            }

            var paymentAttempts = await _PaymentAttemptRepo
                .GetTable()
                .Where(x => x.InvoiceId == invoice.Id)
                .OrderBy(x => x.AttemptedAt)
                .ToListAsync();

            
            if (paymentAttempts.Count < 4)
                throw new InvalidOperationException(
                    "The retry attempts have not been exhausted.");

           
            if (paymentAttempts.Any(
                x => x.Status == PaymentAttemptStatus.Succeeded))
            {
                throw new InvalidOperationException(
                    "A grace period cannot be created because a successful payment attempt exists.");
            }

            var initialAttempt = paymentAttempts[0];
            var retryAttempt1 = paymentAttempts[1];
            var retryAttempt2 = paymentAttempts[2];
            var retryAttempt3 = paymentAttempts[3];

            
            if (initialAttempt.Status != PaymentAttemptStatus.Failed ||
                retryAttempt1.Status != PaymentAttemptStatus.Failed ||
                retryAttempt2.Status != PaymentAttemptStatus.Failed ||
                retryAttempt3.Status != PaymentAttemptStatus.Failed)
            {
                throw new InvalidOperationException(
                    "The required payment retry attempts have not all failed.");
            }

            var retry1RequiredDate =
                initialAttempt.AttemptedAt.AddDays(
                    systemConfig.RetryDay1);

            var retry2RequiredDate =
                initialAttempt.AttemptedAt.AddDays(
                    systemConfig.RetryDay2);

            var retry3RequiredDate =
                initialAttempt.AttemptedAt.AddDays(
                    systemConfig.RetryDay3);

            if (retryAttempt1.AttemptedAt < retry1RequiredDate)
                throw new InvalidOperationException(
                    "The first retry attempt has not reached its configured retry day.");

            if (retryAttempt2.AttemptedAt < retry2RequiredDate)
                throw new InvalidOperationException(
                    "The second retry attempt has not reached its configured retry day.");

            if (retryAttempt3.AttemptedAt < retry3RequiredDate)
                throw new InvalidOperationException(
                    "The third retry attempt has not reached its configured retry day.");

            var existingActiveGracePeriod = await _GracePeriodRepo
                .GetTable()
                .FirstOrDefaultAsync(
                    x => x.SubscriptionId == subscription.Id &&
                         x.Status == GracePeriodStatus.Active);

            if (existingActiveGracePeriod != null)
                throw new InvalidOperationException(
                    "An active grace period already exists for this subscription.");

            
            var startDate = DateTime.UtcNow;

            var endDate = startDate.AddDays(
                systemConfig.GracePeriodDays);

            var entity = _mapper.Map<TbGracePeriod>(dto);

            entity.Id = Guid.NewGuid();

            entity.SubscriptionId = subscription.Id;
            entity.InvoiceId = invoice.Id;

            entity.StartDate = startDate;
            entity.EndDate = endDate;

            entity.Status = GracePeriodStatus.Active;

            await _GracePeriodRepo.AddAsync(
                entity,
                currentUserId);

            return entity.Id;
        }
    }
}