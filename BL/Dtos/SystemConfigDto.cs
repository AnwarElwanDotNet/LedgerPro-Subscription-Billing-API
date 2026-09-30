using BL.Dtos.Base;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class SystemConfigDto : BaseDto
    {
        [Required(ErrorMessage = "Trial duration is required.")]
        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Trial duration cannot be negative.")]
        public int TrialDurationDays { get; set; }

        [Required(ErrorMessage = "Grace period duration is required.")]
        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Grace period duration cannot be negative.")]
        public int GracePeriodDays { get; set; }

        [Required(ErrorMessage = "Retry day 1 is required.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Retry day 1 must be greater than 0.")]
        public int RetryDay1 { get; set; }

        [Required(ErrorMessage = "Retry day 2 is required.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Retry day 2 must be greater than 0.")]
        public int RetryDay2 { get; set; }

        [Required(ErrorMessage = "Retry day 3 is required.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Retry day 3 must be greater than 0.")]
        public int RetryDay3 { get; set; }

        public bool AllowAccessDuringGracePeriod { get; set; }
    }
}