using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entity
{
    public class AspNetUserRole
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }

        public virtual AspNetUser User { get; set; }
        public virtual AspNetRole Role { get; set; }
    }
}
