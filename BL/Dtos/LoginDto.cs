using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BL.Dtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "EmailRequired")]
        [EmailAddress(ErrorMessage = "EmailInvalid")]
        [StringLength(256, ErrorMessage = "EmailMaxLength")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "PasswordRequired")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "PasswordLength")]
        public string Password { get; set; } = null!;

        [StringLength(500, ErrorMessage = "ReturnUrlMaxLength")]
        public string? ReturnUrl { get; set; }
    }
}
