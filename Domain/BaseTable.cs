using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class BaseTable
    {
       public Guid Id { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int CurrentState { get; set; }
    }
}
