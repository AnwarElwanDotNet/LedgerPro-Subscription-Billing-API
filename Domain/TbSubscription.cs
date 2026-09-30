using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbSubscription : BaseTable
    {
        public TbSubscription()
        {
            BillingPeriod = new HashSet<TbBillingPeriod>();
            CancellationRecord = new HashSet<TbCancellationRecord>();
            GracePeriod = new HashSet<TbGracePeriod>();
            Invoice = new HashSet<TbInvoice>();
            UserSeat = new HashSet<TbUserSeat>();
        }
        public Guid OrganizationId { get; set; }
        public Guid PlanId { get; set; }
        public SubscriptionStatus Status { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? CurrentPeriodStart { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }

        public TbOrganization Organization { get; set; }
        public TbPlan Plan { get; set; }
        public ICollection<TbBillingPeriod> BillingPeriod { get; set; }
        public ICollection<TbCancellationRecord> CancellationRecord { get; set; }
        public ICollection<TbGracePeriod> GracePeriod { get; set; }
        public ICollection<TbInvoice> Invoice { get; set; }
        public TbPendingPlanChange PendingPlanChange { get; set; }
        public ICollection<TbUserSeat> UserSeat { get; set; }
    }
}
