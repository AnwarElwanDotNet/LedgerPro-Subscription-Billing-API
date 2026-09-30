
using BL.Dtos;
using Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BL.Contract
{
    public interface IBillingPeriod : IBaseService<TbBillingPeriod, BillingPeriodDto>
    {
        Task<BillingPeriodDto> GetByIdAsync(Guid id);

        Task<IEnumerable<BillingPeriodDto>> GetBySubscriptionIdAsync(Guid subscriptionId);

        Task<Guid> AddAsync(BillingPeriodDto dto);
    }
}

