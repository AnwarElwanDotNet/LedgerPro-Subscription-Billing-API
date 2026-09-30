using BL.Dtos.Base;
using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class InvoiceDto : BaseDto
    {
        [Required(ErrorMessage = "Organization ID is required.")]
        public Guid OrganizationId { get; set; }

        public string? OrganizationName { get; set; }

        [Required(ErrorMessage = "Subscription ID is required.")]
        public Guid SubscriptionId { get; set; }

        public string? SubscriptionStatus { get; set; }

        [Required(ErrorMessage = "Plan ID is required.")]
        public Guid PlanId { get; set; }

        public string? PlanName { get; set; }
        public string? PlanDescription { get; set; }

        public Guid? BillingPeriodId { get; set; }

        public DateTime? BillingPeriodStart { get; set; }
        public DateTime? BillingPeriodEnd { get; set; }
        public BillingPeriodStatus? BillingPeriodStatus { get; set; }

        [Required(ErrorMessage = "Billing duration is required.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Billing duration must be greater than zero.")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Invoice start date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Invoice end date is required.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Plan name snapshot is required.")]
        [StringLength(200,
            ErrorMessage = "Plan name snapshot cannot exceed 200 characters.")]
        public string PlanNameSnapshot { get; set; } = string.Empty;

        [Required(ErrorMessage = "Plan user limit snapshot is required.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Plan user limit snapshot must be greater than zero.")]
        public int PlanUserLimitSnapshot { get; set; }

        [Required(ErrorMessage = "Invoice amount is required.")]
        [Range(typeof(decimal), "0.01", "999999999999.99",
            ErrorMessage = "Invoice amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        [StringLength(3, MinimumLength = 3,
            ErrorMessage = "Currency must be exactly 3 characters.")]
        public string Currency { get; set; } = string.Empty;

        [Required(ErrorMessage = "Invoice status is required.")]
        public InvoiceStatus Status { get; set; }

        [Required(ErrorMessage = "Invoice type is required.")]
        public InvoiceType InvoiceType { get; set; }
    }
}