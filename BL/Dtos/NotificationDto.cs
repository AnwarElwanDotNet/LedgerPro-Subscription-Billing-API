using BL.Dtos.Base;
using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BL.Dtos
{
    public class NotificationDto : BaseDto
    {
        [Required(ErrorMessage = "Organization ID is required.")]
        public Guid OrganizationId { get; set; }
        public string? OrganizationName { get; set; }

        public Guid? UserId { get; set; }
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? Email { get; set; }

        [Required(ErrorMessage = "Notification description is required.")]
        [StringLength(1000, ErrorMessage = "Notification description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Related entity type is required.")]
        public EntityType RelatedEntityType { get; set; }

        [Required(ErrorMessage = "Related entity ID is required.")]
        public Guid RelatedEntityId { get; set; }

        public bool IsRead { get; set; }

        [Required(ErrorMessage = "Notification type is required.")]
        public NotificationType Type { get; set; }
    }
}