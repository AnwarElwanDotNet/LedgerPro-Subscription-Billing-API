using BL.Dtos.Base;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class PlanPricingDto : BaseDto
    {
        [Required(ErrorMessage = "Plan ID is required.")]
        public Guid PlanId { get; set; }
        public string? PlanName { get; set; }
        public string? Description { get; set; }
        public decimal? CurrentPrice { get; set; }
        public string? PlanCurrency { get; set; }
        public int? Duration { get; set; }
        public int? MaxUsers { get; set; }
        public bool? IsActive { get; set; }
        [Required(ErrorMessage = "Plan price is required.")]
        [Range(typeof(decimal), "0.01", "999999999999.99",
            ErrorMessage = "Plan price must be greater than zero.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        [StringLength(3, MinimumLength = 3,
            ErrorMessage = "Currency must be exactly 3 characters.")]
        public string Currency { get; set; } = string.Empty;

        [Required(ErrorMessage = "Effective date is required.")]
        public DateTime EffectiveDate { get; set; }
    }
}