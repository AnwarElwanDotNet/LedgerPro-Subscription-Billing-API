using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BL.Dtos
{
    public class UserResultDto
    {
        [Required(ErrorMessage = "Success status is required.")]
        public bool Success { get; set; }

        [StringLength(1000, ErrorMessage = "Token cannot exceed 1000 characters.")]
        public string? Token { get; set; }

        [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters.")]
        public string? Message { get; set; }

        public IEnumerable<string>? Errors { get; set; } = new List<string>();
    }
}
