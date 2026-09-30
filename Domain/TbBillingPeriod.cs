using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbBillingPeriod : BaseTable       
    {
        public TbBillingPeriod()
        {
            Invoice = new HashSet<TbInvoice>();
        }
        public Guid SubscriptionId { get; set; }
        public Guid PlanId { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public BillingPeriodStatus Status { get; set; }

        public TbSubscription Subscription { get; set; }
        public TbPlan Plan { get; set; }
        public ICollection<TbInvoice> Invoice { get; set; }
    }
}
