using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbPlanPricing : BaseTable
    {
        public Guid PlanId { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public DateTime EffectiveDate { get; set; }

        public TbPlan Plan { get; set; }
    }
}
