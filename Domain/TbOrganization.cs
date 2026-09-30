using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbOrganization : BaseTable
    {
        public TbOrganization()
        {
            CancellationRecord = new HashSet<TbCancellationRecord>();
            Invoice = new HashSet<TbInvoice>();
            Subscription = new HashSet<TbSubscription>();
            Trial = new HashSet<TbTrial>();
            UserSeat = new HashSet<TbUserSeat>();
            Notification = new HashSet<TbNotification>();
        }
        public string OrganizationName { get; set; }

        public ICollection<TbCancellationRecord> CancellationRecord { get; set; }
        public ICollection<TbInvoice> Invoice { get; set; }
        public ICollection<TbSubscription> Subscription { get; set; }
        public ICollection<TbTrial> Trial { get; set; }
        public ICollection<TbUserSeat> UserSeat { get; set; }
        public ICollection<TbNotification> Notification { get; set; }
    }
}
