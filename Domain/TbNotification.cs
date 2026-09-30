using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbNotification : BaseTable
    {
        public Guid OrganizationId { get; set; }
        public Guid? UserId { get; set; }
        public string Description { get; set; }
        public EntityType RelatedEntityType { get; set; }
        public Guid RelatedEntityId { get; set; }
        public bool IsRead { get; set; }
        public NotificationType Type { get; set; }
        public TbOrganization Organization { get; set; }
    }
}
