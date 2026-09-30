using AutoMapper;
using BL.Contract;
using BL.Dtos;
using BL.Exceptions;
using DAL.Contracts;
using Domain;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BL.Services
{
    public class PlanService : BaseService<TbPlan, PlanDto>, IPlan
    {
        private readonly ITableRepository<TbPlan> _PlanRepo;
        private readonly IAuditLog _auditLog;
        private readonly IMapper _Mapper;
        private readonly IUserService _userService;

        public PlanService(
            ITableRepository<TbPlan> tableRepository,
            IMapper mapper,
            IUserService userService,
            IAuditLog auditLog)
            : base(tableRepository, mapper, userService)
        {
            _PlanRepo = tableRepository;
            _Mapper = mapper;
            _userService = userService;
            _auditLog = auditLog;
        }

        public async Task<List<PlanDto>> GetActivePlansAsync()
        {
            var currentUserId = _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Authenticated user is required.");

            var currentUser =
                await _userService.GetUserByIdAsync(
                    currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "Current user was not found.");

            var isPlatformAdmin = currentUser.Roles
                .Any(r => string.Equals(
                    r,
                    "Platform Admin",
                    StringComparison.OrdinalIgnoreCase));

            var isCustomerAdmin = currentUser.Roles
                .Any(r => string.Equals(
                    r,
                    "Customer Admin",
                    StringComparison.OrdinalIgnoreCase));

            if (!isPlatformAdmin && !isCustomerAdmin)
                throw new ForbiddenAccessException(
                    "You are not authorized to view active plans.");

            var plans = await _PlanRepo
                .GetTable()
                .Where(x =>
                    x.CurrentState > 0 &&
                    x.IsActive)
                .ToListAsync();

            return _Mapper.Map<List<PlanDto>>(plans);
        }

        public async Task ActivateAsync(Guid id)
        {
            var currentUserId = _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Authenticated user is required.");

            var currentUser =
                await _userService.GetUserByIdAsync(
                    currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "Current user was not found.");

            var isPlatformAdmin = currentUser.Roles
                .Any(r => string.Equals(
                    r,
                    "Platform Admin",
                    StringComparison.OrdinalIgnoreCase));

            if (!isPlatformAdmin)
                throw new ForbiddenAccessException(
                    "Only Platform Admin can activate a plan.");

            var plan =
                await _PlanRepo
                    .GetTable()
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == id &&
                            x.CurrentState > 0);

            if (plan == null)
                throw new KeyNotFoundException(
                    "Plan not found.");

            plan.IsActive = true;

            await _PlanRepo.UpdateAsync(
                plan,
                currentUserId);

            await _auditLog.AddAsync(
                new AuditLogDto
                {
                    Action = "PlanActivated",
                    EntityType = EntityType.Plan,
                    EntityId = plan.Id
                });
        }

        public async Task DeactivateAsync(Guid id)
        {
            var currentUserId = _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Authenticated user is required.");

            var currentUser =
                await _userService.GetUserByIdAsync(
                    currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "Current user was not found.");

            var isPlatformAdmin = currentUser.Roles
                .Any(r => string.Equals(
                    r,
                    "Platform Admin",
                    StringComparison.OrdinalIgnoreCase));

            if (!isPlatformAdmin)
                throw new ForbiddenAccessException(
                    "Only Platform Admin can deactivate a plan.");

            var plan =
                await _PlanRepo
                    .GetTable()
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == id &&
                            x.CurrentState > 0);

            if (plan == null)
                throw new KeyNotFoundException(
                    "Plan not found.");

            plan.IsActive = false;

            await _PlanRepo.UpdateAsync(
                plan,
                currentUserId);

            await _auditLog.AddAsync(
                new AuditLogDto
                {
                    Action = "PlanDeactivated",
                    EntityType = EntityType.Plan,
                    EntityId = plan.Id
                });
        }
    }
}