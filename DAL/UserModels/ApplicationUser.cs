using Domain;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.UserModels
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public ApplicationUser()
        {
            AuditLog = new HashSet<TbAuditLog>();
            CancellationRecord = new HashSet<TbCancellationRecord>();
            Notification = new HashSet<TbNotification>();
            UserSeat = new HashSet<TbUserSeat>();
            RefreshToken = new HashSet<TbRefreshToken>();
        }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int CurrentState { get; set; } = 1;
        public Guid? OrganizationId { get; set; }
        public ICollection<TbAuditLog> AuditLog { get; set; }
        public ICollection<TbCancellationRecord> CancellationRecord { get; set; }
        public ICollection<TbNotification> Notification { get; set; }
        public ICollection<TbUserSeat> UserSeat { get; set; }
        public ICollection<TbRefreshToken> RefreshToken { get; set; }

        public TbOrganization Organization { get; set; }
    }
}
