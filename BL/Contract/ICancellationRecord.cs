
using BL.Dtos;
using Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BL.Contract
{
    public interface ICancellationRecord
        : IBaseService<TbCancellationRecord, CancellationRecordDto>
    {
        Task<CancellationRecordDto> GetByIdAsync(Guid id);

        Task<IEnumerable<CancellationRecordDto>>
            GetBySubscriptionIdAsync(Guid subscriptionId);

        Task<Guid> AddAsync(
            CancellationRecordDto dto,
            bool refundInitiated);
    }
}

