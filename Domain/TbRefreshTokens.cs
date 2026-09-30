using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class TbRefreshToken : BaseTable
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool Revoked { get; set; }
    }
}
