using BL.Dtos;
using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Contract
{
    public interface IPayment : IBaseService<TbPayment, PaymentDto>
    {
        Task<PaymentDto?> GetByIdAsync(Guid id);

        Task<List<PaymentDto>> GetByInvoiceIdAsync(Guid invoiceId);

        Task<List<PaymentDto>> GetByOrganizationIdAsync(Guid organizationId);

        Task<Guid> ProcessPaymentAsync(PaymentDto dto);

        Task<bool> HandlePaymentResultAsync(PaymentDto dto);
    }
}
