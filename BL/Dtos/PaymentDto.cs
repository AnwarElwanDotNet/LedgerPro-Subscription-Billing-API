using BL.Dtos.Base;
using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class PaymentDto : BaseDto
    {
        [Required(ErrorMessage = "Invoice ID is required.")]
        public Guid InvoiceId { get; set; }
        public InvoiceStatus? InvoiceStatus { get; set; }
        public InvoiceType? InvoiceType { get; set; }
        public string? PlanNameSnapshot { get; set; }
        public string? PlanName { get; set; }

        [Required(ErrorMessage = "Payment amount is required.")]
        [Range(typeof(decimal), "0.01", "999999999999.99",
            ErrorMessage = "Payment amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        [StringLength(3, MinimumLength = 3,
            ErrorMessage = "Currency must be exactly 3 characters.")]
        public string Currency { get; set; } = string.Empty;

        [Required(ErrorMessage = "Payment status is required.")]
        public PaymentStatus Status { get; set; }

        [StringLength(250, ErrorMessage = "Provider reference cannot exceed 250 characters.")]
        public string ProviderReference { get; set; } = string.Empty;

        [Required(ErrorMessage = "Payment date is required.")]
        public DateTime PaymentDate { get; set; }
    }
}