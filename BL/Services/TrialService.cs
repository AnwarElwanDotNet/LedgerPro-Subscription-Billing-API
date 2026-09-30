using AutoMapper;
using BL.Contract;
using BL.Dtos;
using DAL.Contracts;
using Domain;
using Domain.Enums;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using BL.Exceptions;
namespace BL.Services
{
    public class TrialService : BaseService<TbTrial, TrialDto>, ITrial
    {
        private readonly ITableRepository<TbTrial> _Trial;
        private readonly ITableRepository<TbOrganization> _Organization;
        private readonly ITableRepository<TbSystemConfig> _SystemConfig;

        private readonly INotification _Notification;
        private readonly IAuditLog _AuditLog;

        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public TrialService(
            ITableRepository<TbTrial> tableRepository,
            ITableRepository<TbOrganization> organizationRepository,
            ITableRepository<TbSystemConfig> systemConfigRepository,
            INotification notification,
            IAuditLog auditLog,
            IMapper mapper,
            IUserService userService)
            : base(tableRepository, mapper, userService)
        {
            _Trial = tableRepository;
            _Organization = organizationRepository;
            _SystemConfig = systemConfigRepository;

            _Notification = notification;
            _AuditLog = auditLog;

            _mapper = mapper;
            _userService = userService;
        }

       

        public async Task<TrialDto> StartTrialAsync(Guid organizationId)
        {
            if (organizationId == Guid.Empty)
                throw new ArgumentException(
                    "Organization ID is required.",
                    nameof(organizationId));

            var currentUserId = _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            var currentUser = await _userService.GetUserByIdAsync(
                currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            var isPlatformAdmin =
                currentUser.Roles.Contains("Platform Admin");

            var isCustomerAdmin =
                currentUser.Roles.Contains("Customer Admin");

           

            if (!isPlatformAdmin && !isCustomerAdmin)
                throw new ForbiddenAccessException(
                    "You are not authorized to start a trial.");

         

            if (!isPlatformAdmin &&
     currentUser.OrganizationId != organizationId)
            {
                throw new ForbiddenAccessException(
                    "You are not authorized to start a trial for this organization.");
            }

        

            var organization = await _Organization.GetFirstOrDefaultAsync(
                x => x.Id == organizationId &&
                     x.CurrentState > 0);

            if (organization == null)
                throw new KeyNotFoundException(
                    "Organization not found.");

         

            var config = await _SystemConfig.GetFirstOrDefaultAsync(
                x => x.CurrentState > 0);

            if (config == null)
                throw new KeyNotFoundException(
                    "System configuration was not found.");

          

            if (config.TrialDurationDays <= 0)
                throw new InvalidOperationException(
                    "Trial is not enabled.");

           

            var startDate = DateTime.UtcNow;

            var trial = new TbTrial
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                StartDate = startDate,
                EndDate = startDate.AddDays(config.TrialDurationDays),
                Status = TrialStatus.Active
            };

            var correlationId = Guid.NewGuid();

          

            await _Trial.ExecuteInTransactionAsync(
                async () =>
                {
                  

                    var previousTrial = await _Trial.GetFirstOrDefaultAsync(
                        x => x.OrganizationId == organizationId);

                    if (previousTrial != null)
                        throw new InvalidOperationException(
                            "This organization is not eligible for a trial.");

                

                    await _Trial.AddAsync(
                        trial,
                        currentUserId);

                  

                    var notification = new NotificationDto
                    {
                        OrganizationId = organizationId,
                        UserId = currentUserId,
                        Description = "Trial started successfully.",
                        RelatedEntityType = EntityType.Trial,
                        RelatedEntityId = trial.Id,
                        IsRead = false,
                        Type = NotificationType.TrialStarted
                    };

                    await _Notification.AddAsync(notification);

                  

                    var audit = new AuditLogDto
                    {
                        UserId = currentUserId,
                        Action = "TrialStarted",
                        EntityType = EntityType.Trial,
                        EntityId = trial.Id,
                        Metadata = JsonSerializer.Serialize(new
                        {
                            OrganizationId = organizationId,
                            StartDate = trial.StartDate,
                            EndDate = trial.EndDate
                        }),
                        CorrelationId = correlationId
                    };

                    await _AuditLog.AddAsync(audit);
                },
                System.Data.IsolationLevel.Serializable);

          

            var result = _mapper.Map<TrialDto>(trial);

            result.OrganizationName =
                organization.OrganizationName;

            return result;
        }
      

        public async Task<TrialDto> GetCurrentTrialAsync(Guid organizationId)
        {
            if (organizationId == Guid.Empty)
                throw new ArgumentException(
                    "Organization ID is required.",
                    nameof(organizationId));

            var currentUserId = _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            var currentUser = await _userService.GetUserByIdAsync(
                currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            var isPlatformAdmin =
                currentUser.Roles.Contains("Platform Admin");

            var isCustomerAdmin =
                currentUser.Roles.Contains("Customer Admin");

            if (!isPlatformAdmin && !isCustomerAdmin)
                throw new ForbiddenAccessException(
                    "You are not authorized to access this organization's trial.");

          

            if (!isPlatformAdmin &&
      currentUser.OrganizationId != organizationId)
            {
                throw new ForbiddenAccessException(
                    "You are not authorized to access this organization's trial.");
            }

          

            var organization = await _Organization.GetFirstOrDefaultAsync(
                x => x.Id == organizationId &&
                     x.CurrentState > 0);

            if (organization == null)
                throw new KeyNotFoundException(
                    "Organization not found.");

           
            var now = DateTime.UtcNow;

            var trial = await _Trial.GetFirstOrDefaultAsync(
                x => x.OrganizationId == organizationId &&
                     x.Status == TrialStatus.Active &&
                     x.StartDate <= now &&
                     x.EndDate > now);

            if (trial == null)
                throw new KeyNotFoundException(
                    "No active trial was found for this organization.");

           

            var result = _mapper.Map<TrialDto>(trial);

            result.OrganizationName =
                organization.OrganizationName;

            return result;
        }
       

        public async Task<TrialDto> ExpireTrialAsync(Guid trialId)
        {
            if (trialId == Guid.Empty)
                throw new ArgumentException(
                    "Trial ID is required.",
                    nameof(trialId));

            var currentUserId = _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            var currentUser = await _userService.GetUserByIdAsync(
                currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            var isPlatformAdmin =
                currentUser.Roles.Contains("Platform Admin");

            var isCustomerAdmin =
                currentUser.Roles.Contains("Customer Admin");

          

            if (!isPlatformAdmin && !isCustomerAdmin)
                throw new ForbiddenAccessException(
                    "You are not authorized to expire a trial.");

            

            var trial = await _Trial.GetFirstOrDefaultAsync(
                x => x.Id == trialId);

            if (trial == null)
                throw new KeyNotFoundException(
                    "Trial not found.");

         

            if (!isPlatformAdmin &&
      currentUser.OrganizationId != trial.OrganizationId)
            {
                throw new ForbiddenAccessException(
                    "You are not authorized to expire this trial.");
            }

          

            if (trial.Status == TrialStatus.Expired)
                throw new InvalidOperationException(
                    "Trial is already expired.");

            if (trial.Status != TrialStatus.Active)
                throw new InvalidOperationException(
                    "Only an active trial can be expired.");

        

            var now = DateTime.UtcNow;

            if (now < trial.EndDate)
                throw new InvalidOperationException(
                    "Trial has not reached its expiration time.");

        

            var organization = await _Organization.GetFirstOrDefaultAsync(
                x => x.Id == trial.OrganizationId &&
                     x.CurrentState > 0);

            if (organization == null)
                throw new KeyNotFoundException(
                    "Organization not found.");

          

            trial.Status = TrialStatus.Expired;

            var correlationId = Guid.NewGuid();

           

            await _Trial.ExecuteInTransactionAsync(async () =>
            {
               

                await _Trial.UpdateAsync(
                    trial,
                    currentUserId);

               

                var notification = new NotificationDto
                {
                    OrganizationId = trial.OrganizationId,
                    UserId = currentUserId,
                    Description = "Trial ended.",
                    RelatedEntityType = EntityType.Trial,
                    RelatedEntityId = trial.Id,
                    IsRead = false,
                    Type = NotificationType.TrialEnded
                };

                await _Notification.AddAsync(notification);

               

                var audit = new AuditLogDto
                {
                    UserId = currentUserId,
                    Action = "TrialExpired",
                    EntityType = EntityType.Trial,
                    EntityId = trial.Id,
                    Metadata = JsonSerializer.Serialize(new
                    {
                        OrganizationId = trial.OrganizationId,
                        StartDate = trial.StartDate,
                        EndDate = trial.EndDate,
                        ExpiredAt = now
                    }),
                    CorrelationId = correlationId
                };

                await _AuditLog.AddAsync(audit);
            });

           

            var result = _mapper.Map<TrialDto>(trial);

            result.OrganizationName =
                organization.OrganizationName;

            return result;
        }
    }
}