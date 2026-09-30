using BL.Dtos.Base;
using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class TrialDto : BaseDto
    {
        [Required(ErrorMessage = "Organization ID is required.")]
        public Guid OrganizationId { get; set; }
        public string? OrganizationName { get; set; }

        [Required(ErrorMessage = "Trial start date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Trial end date is required.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Trial status is required.")]
        public TrialStatus Status { get; set; }
    }
}