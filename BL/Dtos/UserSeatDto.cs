using BL.Dtos.Base;
using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class UserSeatDto : BaseDto
    {
        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; }
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "Organization ID is required.")]
        public Guid OrganizationId { get; set; }
        public string? OrganizationName { get; set; }

        [Required(ErrorMessage = "Subscription ID is required.")]
        public Guid SubscriptionId { get; set; }
        public SubscriptionStatus? SubscriptionStatus { get; set; }
        public string? PlanName { get; set; }
        public string? Description { get; set; }
        public string? Currency { get; set; }

        [Required(ErrorMessage = "User seat status is required.")]
        public UserSeatStatus Status { get; set; }
    }
}