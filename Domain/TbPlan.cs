using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbPlan : BaseTable
    {
        public TbPlan()
        {
            BillingPeriod = new HashSet<TbBillingPeriod>();
            Invoice = new HashSet<TbInvoice>();
            PendingPlanChange = new HashSet<TbPendingPlanChange>();
            PlanPricing = new HashSet<TbPlanPricing>();
            Subscription = new HashSet<TbSubscription>();
        }
        public string PlanName { get; set; }
        public string Description { get; set; }
        public decimal CurrentPrice { get; set; }
        public string Currency { get; set; }
        public int Duration { get; set; }
        public int MaxUsers { get; set; }
        public bool IsActive { get; set; }

        public ICollection<TbBillingPeriod> BillingPeriod { get; set; }
        public ICollection<TbInvoice> Invoice { get; set; }
        public ICollection<TbPendingPlanChange> PendingPlanChange { get; set; }
        public ICollection<TbPlanPricing> PlanPricing { get; set; }
        public ICollection<TbSubscription> Subscription { get; set; }
    }
}
