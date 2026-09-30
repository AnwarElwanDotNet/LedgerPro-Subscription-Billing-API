using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbSystemConfig : BaseTable
    {
        public int TrialDurationDays { get; set; }
        public int GracePeriodDays { get; set; }
        public int RetryDay1 { get; set; }
        public int RetryDay2 { get; set; }
        public int RetryDay3 { get; set; }
        public bool AllowAccessDuringGracePeriod { get; set; }
    }
}
