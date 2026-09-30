using BL.Dtos;
using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Contract
{
    public interface IGracePeriod : IBaseService<TbGracePeriod,GracePeriodDto>
    {
        Task<GracePeriodDto> GetByIdAsync(Guid id);

        Task<IEnumerable<GracePeriodDto>>
            GetBySubscriptionIdAsync(Guid subscriptionId);

        Task<Guid> AddAsync(GracePeriodDto dto);
    }
}
