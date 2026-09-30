using BL.Dtos.Base;
using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class AuditLogDto : BaseDto
    {
        public Guid? UserId { get; set; }
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        [Required(ErrorMessage = "Audit action is required.")]
        [StringLength(250, ErrorMessage = "Audit action cannot exceed 250 characters.")]
        public string Action { get; set; } = string.Empty;

        [Required(ErrorMessage = "Entity type is required.")]
        public EntityType EntityType { get; set; }

        [Required(ErrorMessage = "Entity ID is required.")]
        public Guid EntityId { get; set; }

        [StringLength(4000, ErrorMessage = "Metadata cannot exceed 4000 characters.")]
        public string Metadata { get; set; } = string.Empty;

        public Guid? CorrelationId { get; set; }

      
        public DateTime Timestamp { get; set; }
    }
}