using BL.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Dtos
{
    public class ForgotPasswordResultDto : BaseDto
    {
        public string Email { get; set; } = string.Empty;
        public string? Token { get; set; }
    }
}
