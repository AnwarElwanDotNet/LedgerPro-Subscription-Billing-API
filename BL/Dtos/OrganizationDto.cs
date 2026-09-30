using BL.Dtos.Base;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class OrganizationDto : BaseDto
    {
        [Required(ErrorMessage = "Organization name is required.")]
        [StringLength(200, ErrorMessage = "Organization name cannot exceed 200 characters.")]
        public string OrganizationName { get; set; } = string.Empty;
    }
}