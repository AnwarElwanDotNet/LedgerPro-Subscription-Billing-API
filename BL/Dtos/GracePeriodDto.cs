using BL.Dtos.Base;
using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class GracePeriodDto : BaseDto
    {
        [Required(ErrorMessage = "Subscription ID is required.")]
        public Guid SubscriptionId { get; set; }
        public string? OrganizationName { get; set; }

        [Required(ErrorMessage = "Invoice ID is required.")]
        public Guid InvoiceId { get; set; }
        public string? PlanName { get; set; }
        public int? Duration { get; set; }
    
        public string? PlanNameSnapshot { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }

        [Required(ErrorMessage = "Grace period start date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Grace period end date is required.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Grace period status is required.")]
        public GracePeriodStatus Status { get; set; }

        [StringLength(500, ErrorMessage = "Grace period reason cannot exceed 500 characters.")]
        public string Reason { get; set; } = string.Empty;
    }
}