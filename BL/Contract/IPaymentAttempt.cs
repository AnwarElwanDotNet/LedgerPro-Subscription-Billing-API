using BL.Dtos;
using Domain;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BL.Contract
{
    public interface IPaymentAttempt : IBaseService<TbPaymentAttempt, PaymentAttemptDto>
    {
        Task<Guid> AddAsync(PaymentAttemptDto dto);

        Task<Guid> RecordAsync(PaymentAttemptDto dto);

        Task<PaymentAttemptDto?> GetByIdAsync(Guid id);

        Task<List<PaymentAttemptDto>> GetByInvoiceIdAsync(Guid invoiceId);

        Task<List<PaymentAttemptDto>> GetFailedAsync();
    }
}