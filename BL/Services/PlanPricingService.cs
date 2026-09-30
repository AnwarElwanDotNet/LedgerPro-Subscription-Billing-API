using AutoMapper;
using BL.Contract;
using BL.Dtos;
using BL.Exceptions;
using DAL.Contracts;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BL.Services
{
    public class PlanPricingService : BaseService<TbPlanPricing, PlanPricingDto>, IPlanPricing
    {
        private readonly ITableRepository<TbPlanPricing> _PlanPricingRepo;
        private readonly ITableRepository<TbPlan> _PlanRepo;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public PlanPricingService(
            ITableRepository<TbPlanPricing> tableRepository,
            ITableRepository<TbPlan> tableRepository1,
            IMapper mapper,
            IUserService userService)
            : base(tableRepository, mapper, userService)
        {
            _PlanPricingRepo = tableRepository;
            _PlanRepo = tableRepository1;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<Guid> AddAsync(PlanPricingDto dto)
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
                    "Only Platform Admin can configure plan pricing.");

            var plan = await _PlanRepo
    .GetTable()
    .FirstOrDefaultAsync(
        x =>
            x.Id == dto.PlanId &&
            x.CurrentState > 0);

            if (plan == null)
                throw new KeyNotFoundException(
                    "Plan not found.");

            var pricing = _mapper.Map<TbPlanPricing>(dto);

            pricing.Id = Guid.NewGuid();
            pricing.PlanId = dto.PlanId;
            pricing.Price = dto.Price;
            pricing.Currency = dto.Currency;
            pricing.EffectiveDate = dto.EffectiveDate.Kind == DateTimeKind.Utc
                ? dto.EffectiveDate
                : dto.EffectiveDate.ToUniversalTime();

            plan.CurrentPrice = dto.Price;
            plan.Currency = dto.Currency;

            await _PlanPricingRepo.AddAsync(
                pricing,
                currentUserId);

            await _PlanRepo.UpdateAsync(
                plan,
                currentUserId);

            return pricing.Id;
        }
        public async Task<List<PlanPricingDto>> GetByPlanIdAsync(Guid planId)
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
                    "You are not authorized to view plan pricing.");

            var plan = await _PlanRepo
    .GetTable()
    .FirstOrDefaultAsync(
        x =>
            x.Id == planId &&
            x.CurrentState > 0);

            if (plan == null)
                throw new KeyNotFoundException(
                    "Plan not found.");

            var pricingList = await _PlanPricingRepo
     .GetTable()
     .Include(x => x.Plan)
     .Where(x =>
         x.PlanId == planId &&
         x.CurrentState > 0)
     .OrderByDescending(x => x.EffectiveDate)
     .ToListAsync();

            return _mapper.Map<List<PlanPricingDto>>(pricingList);
        }
    }
}