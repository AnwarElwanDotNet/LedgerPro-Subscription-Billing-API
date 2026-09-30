using BL.Dtos.Base;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class PlanDto : BaseDto
    {
        [Required(ErrorMessage = "Plan name is required.")]
        [StringLength(200, ErrorMessage = "Plan name cannot exceed 200 characters.")]
        public string PlanName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Plan description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Plan price is required.")]
        [Range(typeof(decimal), "0.01", "999999999999.99",
            ErrorMessage = "Plan price must be greater than zero.")]
        public decimal CurrentPrice { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        [StringLength(3, MinimumLength = 3,
            ErrorMessage = "Currency must be exactly 3 characters.")]
        public string Currency { get; set; } = string.Empty;

        [Required(ErrorMessage = "Plan duration is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Plan duration must be greater than zero.")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Maximum users is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Maximum users must be greater than zero.")]
        public int MaxUsers { get; set; }

        public bool IsActive { get; set; }
    }
}