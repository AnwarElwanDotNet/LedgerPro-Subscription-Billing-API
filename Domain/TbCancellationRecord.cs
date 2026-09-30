using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbCancellationRecord : BaseTable
    {
        public Guid OrganizationId { get; set; }
        public Guid SubscriptionId { get; set; }
        public Guid PerformedBy { get; set; }
        public DateTime PerformedAt { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string Reason { get; set; }
        public CancellationType Type { get; set; }
        public bool AccessEndedImmediately { get; set; }
        public bool RefundInitiated { get; set; }

        public TbOrganization Organization { get; set; }
        public TbSubscription Subscription { get; set; }
    }
}
