using BL.Dtos;
using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Contract
{
    public interface IPendingPlanChange : IBaseService<TbPendingPlanChange,PendingPlanChangeDto>
    {
        Task<Guid> AddAsync(PendingPlanChangeDto dto);

        Task<PendingPlanChangeDto?> GetBySubscriptionIdAsync(Guid subscriptionId);

        Task ApplyAsync(Guid subscriptionId);
    }
}
