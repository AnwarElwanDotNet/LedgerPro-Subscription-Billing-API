using BL.Dtos;
using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Contract
{
    public interface IInvoice : IBaseService<TbInvoice,InvoiceDto>
    {
        Task<InvoiceDto> GetByIdAsync(Guid id);

        Task<IEnumerable<InvoiceDto>> GetBySubscriptionIdAsync(Guid subscriptionId);

        Task<IEnumerable<InvoiceDto>> GetByOrganizationIdAsync(Guid organizationId);

        Task<Guid> AddAsync(InvoiceDto dto);
    }
}
