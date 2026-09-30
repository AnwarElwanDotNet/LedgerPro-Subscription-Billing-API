using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbPaymentAttempt : BaseTable
    {
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public PaymentAttemptStatus Status { get; set; }
        public string ProviderReference { get; set; }
        public DateTime AttemptedAt { get; set; }

        public TbInvoice Invoice { get; set; }
    }
}
