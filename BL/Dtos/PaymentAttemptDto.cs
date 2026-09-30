using BL.Dtos.Base;
using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class PaymentAttemptDto : BaseDto
    {
        [Required(ErrorMessage = "Invoice ID is required.")]
        public Guid InvoiceId { get; set; }
        public string? PlanNameSnapshot { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public string? PlanName { get; set; }
        public InvoiceStatus? InvoiceStatus { get; set; }
        public InvoiceType? InvoiceType { get; set; }
        public string? Currency { get; set; }
        [Required(ErrorMessage = "Payment attempt amount is required.")]
        [Range(typeof(decimal), "0.01", "999999999999.99",
            ErrorMessage = "Payment attempt amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment attempt status is required.")]
        public PaymentAttemptStatus Status { get; set; }

        [StringLength(250, ErrorMessage = "Provider reference cannot exceed 250 characters.")]
        public string ProviderReference { get; set; } = string.Empty;

        [Required(ErrorMessage = "Attempt date is required.")]
        public DateTime AttemptedAt { get; set; }
    }
}