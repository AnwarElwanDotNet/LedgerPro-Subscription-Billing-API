using BL.Dtos.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BL.Dtos
{
    public class ChangeEmailDto : BaseDto
    {
        [Required(ErrorMessage = "New email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        public string NewEmail { get; set; } = null!;
    }
}
