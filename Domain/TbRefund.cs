using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbRefund : BaseTable
    {
        public Guid PaymentId { get; set; }
        public decimal RefundAmount { get; set; }
        public string Currency { get; set; }
        public RefundStatus Status { get; set; }
        public DateTime RefundDate { get; set; }

        public TbPayment Payment { get; set; }
    }
}
