using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbInvoice : BaseTable
    {
        public TbInvoice()
        {
            GracePeriod = new HashSet<TbGracePeriod>();
            PaymentAttempt = new HashSet<TbPaymentAttempt>();
        }
        public Guid OrganizationId { get; set; }
        public Guid SubscriptionId { get; set; }
        public Guid PlanId { get; set; }
        public Guid? BillingPeriodId { get; set; }
        public int Duration { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PlanNameSnapshot { get; set; }
        public int PlanUserLimitSnapshot { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public InvoiceStatus Status { get; set; }
        public InvoiceType InvoiceType { get; set; }
        public TbOrganization Organization { get; set; }
        public TbSubscription Subscription { get; set; }
        public TbPlan Plan { get; set; }
        public TbBillingPeriod BillingPeriod { get; set; }
        public ICollection<TbGracePeriod> GracePeriod { get; set; }
        public TbPayment Payment { get; set; }
        public ICollection<TbPaymentAttempt> PaymentAttempt { get; set; }
    }
}
