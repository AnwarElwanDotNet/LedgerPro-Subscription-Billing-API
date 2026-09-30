using BL.Dtos;
using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Contract
{
    public interface IPlan : IBaseService<TbPlan,PlanDto>
    {
        Task<List<PlanDto>> GetActivePlansAsync();
        Task ActivateAsync(Guid id);
        Task DeactivateAsync(Guid id);
    }
}
