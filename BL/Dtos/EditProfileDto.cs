using BL.Dtos.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BL.Dtos
{
    public class EditProfileDto : BaseDto
    {
        [Required(ErrorMessage = "FirstNameRequired")]
        [StringLength(50, ErrorMessage = "FirstNameMaxLength")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "LastNameRequired")]
        [StringLength(50, ErrorMessage = "LastNameMaxLength")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "PhoneRequired")]
        [Phone(ErrorMessage = "PhoneInvalid")]
        public string Phone { get; set; } = null!;
    }
}
