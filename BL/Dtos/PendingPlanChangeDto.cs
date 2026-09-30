using BL.Dtos.Base;
using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class PendingPlanChangeDto : BaseDto
    {
        [Required(ErrorMessage = "Subscription ID is required.")]
        public Guid SubscriptionId { get; set; }

        public string? OrganizationName { get; set; }
        public string? SubscriptionPlanName { get; set; }
        public SubscriptionStatus? Status { get; set; }

        [Required(ErrorMessage = "Target plan ID is required.")]
        public Guid TargetPlanId { get; set; }

        public string? TargetPlanName { get; set; }
        public string? TargetPlanDescription { get; set; }
        public decimal? TargetPlanPrice { get; set; }
        public string? TargetPlanCurrency { get; set; }
        public int? TargetPlanDuration { get; set; }
        public int? TargetPlanMaxUsers { get; set; }
        public bool TargetPlanIsActive { get; set; }

        [Required(ErrorMessage = "Effective date is required.")]
        public DateTime EffectiveDate { get; set; }

        public bool IsApplied { get; set; }
    }
}