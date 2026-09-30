using BL.Dtos.Base;
using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class CancellationRecordDto : BaseDto
    {
        [Required(ErrorMessage = "Organization ID is required.")]
        public Guid OrganizationId { get; set; }
        public string? OrganizationName { get; set; }

        [Required(ErrorMessage = "Subscription ID is required.")]
        public Guid SubscriptionId { get; set; }
        public string? PlanName { get; set; }
        public string? Description { get; set; }

        public decimal? CurrentPrice { get; set; }

        [Required(ErrorMessage = "Performed by user ID is required.")]
        public Guid PerformedBy { get; set; }

        [Required(ErrorMessage = "Cancellation performed date is required.")]
        public DateTime PerformedAt { get; set; }

        [Required(ErrorMessage = "Cancellation effective date is required.")]
        public DateTime EffectiveDate { get; set; }

        [StringLength(500, ErrorMessage = "Cancellation reason cannot exceed 500 characters.")]
        public string Reason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cancellation type is required.")]
        public CancellationType Type { get; set; }

        public bool AccessEndedImmediately { get; set; }

        public bool RefundInitiated { get; set; }
    }
}