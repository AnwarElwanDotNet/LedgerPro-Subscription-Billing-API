using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbPayment : BaseTable
    {
        public TbPayment()
        {
            Refund = new HashSet<TbRefund>();
        }
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public PaymentStatus Status { get; set; }
        public string ProviderReference { get; set; }
        public DateTime PaymentDate { get; set; }

        public TbInvoice Invoice { get; set; }

        public ICollection<TbRefund> Refund { get; set; }
    }
}
