using AutoMapper;
using BL.Contract;
using BL.Dtos;
using BL.Exceptions;
using DAL.Contracts;
using Domain;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BL.Services
{
    public class SystemConfigService
        : BaseService<TbSystemConfig, SystemConfigDto>, ISystemConfig
    {
        private readonly ITableRepository<TbSystemConfig> _SystemConfig;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IAuditLog _auditLog;

        public SystemConfigService(
            ITableRepository<TbSystemConfig> tableRepository,
            IMapper mapper,
            IUserService userService,
            IAuditLog auditLog)
            : base(tableRepository, mapper, userService)
        {
            _SystemConfig = tableRepository;
            _mapper = mapper;
            _userService = userService;
            _auditLog = auditLog;
        }

        public async Task<SystemConfigDto> GetAsync()
        {
           

            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");
            }

            var user = await _userService.GetUserByIdAsync(
                userId.ToString());

            if (user == null)
            {
                throw new UnauthorizedAccessException(
                    "User not found.");
            }

           

            if (!string.Equals(
                    user.Role,
                    "Platform Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ForbiddenAccessException(
                    "Only Platform Admin can access system configuration.");
            }

            

            var config =
                await _SystemConfig.GetFirstOrDefaultAsync(
                    x => x.CurrentState > 0);

            if (config == null)
            {
                throw new KeyNotFoundException(
                    "System configuration was not found.");
            }

            return _mapper.Map<SystemConfigDto>(config);
        }

        public async Task<SystemConfigDto> UpdateAsync(
            SystemConfigDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

          

            if (dto.RetryDay2 <= dto.RetryDay1)
            {
                throw new InvalidOperationException(
                    "Retry day 2 must be greater than retry day 1.");
            }

            if (dto.RetryDay3 <= dto.RetryDay2)
            {
                throw new InvalidOperationException(
                    "Retry day 3 must be greater than retry day 2.");
            }

         

            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");
            }

            var user = await _userService.GetUserByIdAsync(
                userId.ToString());

            if (user == null)
            {
                throw new UnauthorizedAccessException(
                    "User not found.");
            }

           

            if (!string.Equals(
                    user.Role,
                    "Platform Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ForbiddenAccessException(
                    "Only Platform Admin can update system configuration.");
            }

           

            var config =
                await _SystemConfig.GetFirstOrDefaultAsync(
                    x => x.CurrentState > 0);

            if (config == null)
            {
                throw new KeyNotFoundException(
                    "System configuration was not found.");
            }

        

            var configId = config.Id;
            var currentState = config.CurrentState;
            var createdDate = config.CreatedDate;
            var createdBy = config.CreatedBy;

          

            _mapper.Map(dto, config);

            config.Id = configId;
            config.CurrentState = currentState;
            config.CreatedDate = createdDate;
            config.CreatedBy = createdBy;
            config.UpdatedDate = DateTime.UtcNow;

           

            await _SystemConfig.UpdateAsync(
                config,
                userId);

         

            await _auditLog.AddAsync(
                new AuditLogDto
                {
                    Action = "Update System Configuration",
                    EntityType = EntityType.SystemConfig,
                    EntityId = config.Id,
                    CorrelationId = Guid.NewGuid(),
                    Metadata =
                        $"TrialDurationDays={config.TrialDurationDays}; " +
                        $"GracePeriodDays={config.GracePeriodDays}; " +
                        $"RetryDay1={config.RetryDay1}; " +
                        $"RetryDay2={config.RetryDay2}; " +
                        $"RetryDay3={config.RetryDay3}; " +
                        $"AllowAccessDuringGracePeriod={config.AllowAccessDuringGracePeriod}"
                });

            return _mapper.Map<SystemConfigDto>(config);
        }
    }
}