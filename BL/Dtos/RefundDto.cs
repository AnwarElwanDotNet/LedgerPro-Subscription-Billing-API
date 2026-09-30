using BL.Dtos.Base;
using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class RefundDto : BaseDto
    {
        [Required(ErrorMessage = "Payment ID is required.")]
        public Guid PaymentId { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public InvoiceStatus? InvoiceStatus { get; set; }
        public InvoiceType? InvoiceType { get; set; }
        public string? OrganizationName { get; set; }
        public string? PlanName { get; set; }

        [Required(ErrorMessage = "Refund amount is required.")]
        [Range(typeof(decimal), "0.01", "999999999999.99",
            ErrorMessage = "Refund amount must be greater than zero.")]
        public decimal RefundAmount { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        [StringLength(3, MinimumLength = 3,
            ErrorMessage = "Currency must be exactly 3 characters.")]
        public string Currency { get; set; } = string.Empty;

        [Required(ErrorMessage = "Refund status is required.")]
        public RefundStatus Status { get; set; }

        public DateTime? RefundDate { get; set; }
    }
}