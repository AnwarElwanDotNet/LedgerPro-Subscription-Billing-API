using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbPendingPlanChange : BaseTable
    {
        public Guid SubscriptionId { get; set; }
        public Guid TargetPlanId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public bool IsApplied { get; set; }

        public TbSubscription Subscription { get; set; }
        public TbPlan Plan { get; set; }
    }
}
