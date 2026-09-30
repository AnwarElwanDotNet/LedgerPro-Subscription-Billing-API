using BL.Dtos;
using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Contract
{
    public interface IPlanPricing : IBaseService<TbPlanPricing,PlanPricingDto>
    {
        Task<Guid> AddAsync(PlanPricingDto dto);
        Task<List<PlanPricingDto>> GetByPlanIdAsync(Guid planId);
    }
}
