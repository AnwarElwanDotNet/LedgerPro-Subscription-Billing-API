using BL.Dtos.Base;
using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class SubscriptionDto : BaseDto
    {
        [Required(ErrorMessage = "Organization ID is required.")]
        public Guid OrganizationId { get; set; }
        public string? OrganizationName { get; set; }

        [Required(ErrorMessage = "Plan ID is required.")]
        public Guid PlanId { get; set; }
        public string? PlanName { get; set; }
        public string? Description { get; set; }
        public decimal? CurrentPrice { get; set; }
        public string? Currency { get; set; }
        public int? Duration { get; set; }
        public int? MaxUsers { get; set; }
        public bool? IsActive { get; set; }
        [Required(ErrorMessage = "Subscription status is required.")]
        public SubscriptionStatus Status { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CurrentPeriodStart { get; set; }

        public DateTime? CurrentPeriodEnd { get; set; }
    }
}