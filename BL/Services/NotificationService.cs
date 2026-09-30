using AutoMapper;
using BL.Contract;
using BL.Dtos;
using DAL.Contracts;
using DAL.UserModels;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BL.Exceptions;
namespace BL.Services
{
    public class NotificationService
        : BaseService<TbNotification, NotificationDto>, INotification
    {
        private readonly ITableRepository<TbNotification> _NotificationRepo;
        private readonly ITableRepository<TbOrganization> _OrganizationRepo;
        private readonly IViewRepository<ApplicationUser> _UserRepo;

        private readonly IMapper _Mapper;
        private readonly IUserService _UserService;

        public NotificationService(
            ITableRepository<TbNotification> tableRepository,
            ITableRepository<TbOrganization> organizationRepository,
            IViewRepository<ApplicationUser> userRepository,
            IMapper mapper,
            IUserService userService)
            : base(tableRepository, mapper, userService)
        {
            _NotificationRepo = tableRepository;
            _OrganizationRepo = organizationRepository;
            _UserRepo = userRepository;

            _Mapper = mapper;
            _UserService = userService;
        }

        // ================= GET BY ID =================

        public async Task<NotificationDto?> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            var currentUserId = _UserService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            var currentUser = await _UserService.GetUserByIdAsync(
                currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            var isPlatformAdmin =
     currentUser.Roles.Any(r =>
         string.Equals(
             r,
             "Platform Admin",
             StringComparison.OrdinalIgnoreCase));

            var query =
                from n in _NotificationRepo.GetTable()

                join organization in _OrganizationRepo.GetTable()
                    on n.OrganizationId equals organization.Id

                join user in _UserRepo.GetTable()
                    on n.UserId equals (Guid?)user.Id
                    into userGroup

                from user in userGroup.DefaultIfEmpty()

                where n.Id == id
                && n.CurrentState > 0
                && organization.CurrentState > 0

                select new
                {
                    Notification = n,
                    Organization = organization,
                    User = user
                };

            var result = await query.FirstOrDefaultAsync();

            if (result == null)
                return null;

            var notification = result.Notification;

            if (!isPlatformAdmin &&
     (!currentUser.OrganizationId.HasValue ||
      currentUser.OrganizationId.Value != notification.OrganizationId))
            {
                throw new ForbiddenAccessException(
                    "You are not authorized to access this notification.");
            }

            var dto = _Mapper.Map<NotificationDto>(notification);

            dto.OrganizationName =
                result.Organization.OrganizationName;

            if (result.User != null)
            {
                dto.FirstName = result.User.FirstName;
                dto.LastName = result.User.LastName;
                dto.Email = result.User.Email;
            }

            return dto;
        }
        // ================= GET BY ORGANIZATION =================

        public async Task<List<NotificationDto>> GetByOrganizationIdAsync(
            Guid organizationId)
        {
            if (organizationId == Guid.Empty)
                return new List<NotificationDto>();

            var currentUserId = _UserService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            var currentUser = await _UserService.GetUserByIdAsync(
                currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            var isPlatformAdmin =
      currentUser.Roles.Any(r =>
          string.Equals(
              r,
              "Platform Admin",
              StringComparison.OrdinalIgnoreCase));
            if (!isPlatformAdmin &&
      (!currentUser.OrganizationId.HasValue ||
       currentUser.OrganizationId.Value != organizationId))
            {
                throw new ForbiddenAccessException(
                    "You are not authorized to access notifications for this organization.");
            }

            var organization = await _OrganizationRepo.GetByIdAsync(
                organizationId);
            if (organization == null ||
                organization.CurrentState <= 0)
            {
                return new List<NotificationDto>();
            }

            var result = await (
                from notification in _NotificationRepo.GetTable()

                join user in _UserRepo.GetTable()
                    on notification.UserId equals (Guid?)user.Id
                    into userGroup

                from user in userGroup.DefaultIfEmpty()

                where notification.OrganizationId == organizationId
       && notification.CurrentState > 0

                select new NotificationDto
                {
                    Id = notification.Id,
                    OrganizationId = notification.OrganizationId,

                    OrganizationName =
                        organization.OrganizationName,

                    UserId = notification.UserId,

                    FirstName = user != null
                        ? user.FirstName
                        : null,

                    LastName = user != null
                        ? user.LastName
                        : null,

                    Email = user != null
                        ? user.Email
                        : null,

                    Description = notification.Description,

                    RelatedEntityType =
                        notification.RelatedEntityType,

                    RelatedEntityId =
                        notification.RelatedEntityId,

                    IsRead = notification.IsRead,

                    Type = notification.Type
                }
            ).ToListAsync();

            return result;
        }

        // ================= ADD =================

        public async Task<NotificationDto> AddAsync(
            NotificationDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.OrganizationId == Guid.Empty)
                throw new ArgumentException(
                    "Organization ID is required.",
                    nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new ArgumentException(
                    "Notification description is required.",
                    nameof(dto));

            if (dto.Description.Length > 1000)
                throw new ArgumentException(
                    "Notification description cannot exceed 1000 characters.",
                    nameof(dto));

            if (!Enum.IsDefined(
                    typeof(NotificationType),
                    dto.Type))
            {
                throw new ArgumentException(
                    "Invalid notification type.",
                    nameof(dto));
            }

            if (!Enum.IsDefined(
                    typeof(EntityType),
                    dto.RelatedEntityType))
            {
                throw new ArgumentException(
                    "Invalid related entity type.",
                    nameof(dto));
            }

            if (dto.RelatedEntityId == Guid.Empty)
                throw new ArgumentException(
                    "Related entity ID is required.",
                    nameof(dto));

            var currentUserId = _UserService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            var currentUser = await _UserService.GetUserByIdAsync(
                currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            var isPlatformAdmin =
     currentUser.Roles.Any(r =>
         string.Equals(
             r,
             "Platform Admin",
             StringComparison.OrdinalIgnoreCase));

            if (!isPlatformAdmin &&
       (!currentUser.OrganizationId.HasValue ||
        currentUser.OrganizationId.Value != dto.OrganizationId))
            {
                throw new ForbiddenAccessException(
                    "You are not authorized to create a notification for this organization.");
            }

            var organization = await _OrganizationRepo.GetByIdAsync(
                dto.OrganizationId);

            if (organization == null ||
     organization.CurrentState <= 0)
            {
                throw new KeyNotFoundException(
                    "Organization not found.");
            }

            
            if (dto.UserId.HasValue)
            {
                var targetUser = await _UserRepo.GetTable()
                    .Where(x => x.Id == dto.UserId.Value)
                    .Select(x => new
                    {
                        x.Id,
                        x.OrganizationId
                    })
                    .FirstOrDefaultAsync();

                if (targetUser == null)
                    throw new KeyNotFoundException(
                        "Notification target user not found.");

                if (targetUser.OrganizationId != dto.OrganizationId)
                    throw new InvalidOperationException(
                        "Notification target user does not belong to the specified organization.");
            }

            var notification = new TbNotification
            {
                Id = Guid.NewGuid(),
                CurrentState = 1,
                CreatedDate = DateTime.UtcNow,

                OrganizationId = dto.OrganizationId,
                UserId = dto.UserId,

                Description = dto.Description.Trim(),

                RelatedEntityType = dto.RelatedEntityType,
                RelatedEntityId = dto.RelatedEntityId,

                IsRead = false,

                Type = dto.Type
            };

            await _NotificationRepo.AddAsync(
                notification,
                currentUserId);

          
            var result = await (
                from createdNotification in _NotificationRepo.GetTable()

                join user in _UserRepo.GetTable()
                    on createdNotification.UserId equals (Guid?)user.Id
                    into userGroup

                from user in userGroup.DefaultIfEmpty()

                where createdNotification.Id == notification.Id

                select new NotificationDto
                {
                    Id = createdNotification.Id,

                    OrganizationId =
                        createdNotification.OrganizationId,

                    OrganizationName =
                        organization.OrganizationName,

                    UserId =
                        createdNotification.UserId,

                    FirstName = user != null
                        ? user.FirstName
                        : null,

                    LastName = user != null
                        ? user.LastName
                        : null,

                    Email = user != null
                        ? user.Email
                        : null,

                    Description =
                        createdNotification.Description,

                    RelatedEntityType =
                        createdNotification.RelatedEntityType,

                    RelatedEntityId =
                        createdNotification.RelatedEntityId,

                    IsRead =
                        createdNotification.IsRead,

                    Type =
                        createdNotification.Type
                }
            ).FirstOrDefaultAsync();

            if (result == null)
                throw new InvalidOperationException(
                    "The notification could not be retrieved after creation.");

            return result;
        }
    }
}