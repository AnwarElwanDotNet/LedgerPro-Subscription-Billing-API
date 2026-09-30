using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbUserSeat : BaseTable
    {
        public Guid UserId { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid SubscriptionId { get; set; }
        public UserSeatStatus Status { get; set; }

        public TbOrganization Organization { get; set; }
        public TbSubscription Subscription { get; set; }
    }
}
