using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbGracePeriod : BaseTable
    {
        public Guid SubscriptionId { get; set; }
        public Guid InvoiceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public GracePeriodStatus Status { get; set; }
        public string Reason { get; set; }

        public TbSubscription Subscription { get; set; }
        public TbInvoice Invoice { get; set; }
    }
}
