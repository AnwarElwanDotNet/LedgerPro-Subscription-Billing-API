using BL.Dtos.Base;
using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class BillingPeriodDto : BaseDto
    {
        [Required(ErrorMessage = "Subscription ID is required.")]
        public Guid SubscriptionId { get; set; }
        public string? OrganizationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Plan ID is required.")]
        public Guid PlanId { get; set; }
        public string? PlanName { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Billing period start date is required.")]
        public DateTime Start { get; set; }

        [Required(ErrorMessage = "Billing period end date is required.")]
        public DateTime End { get; set; }

        [Required(ErrorMessage = "Billing period amount is required.")]
        [Range(typeof(decimal), "0.01", "999999999999.99",
            ErrorMessage = "Billing period amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        [StringLength(3, MinimumLength = 3,
            ErrorMessage = "Currency must be exactly 3 characters.")]
        public string Currency { get; set; } = string.Empty;

        [Required(ErrorMessage = "Billing period status is required.")]
        public BillingPeriodStatus Status { get; set; }
    }
}