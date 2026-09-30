using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbTrial : BaseTable
    {
        public Guid OrganizationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TrialStatus Status { get; set; }

        public TbOrganization Organization { get; set; }
    }
}
