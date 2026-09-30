using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbAuditLog : BaseTable
    {
        public Guid? UserId { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public EntityType EntityType { get; set; }
        public Guid EntityId { get; set; }
        public string Metadata { get; set; }
        public Guid? CorrelationId { get; set; }
    }
}
