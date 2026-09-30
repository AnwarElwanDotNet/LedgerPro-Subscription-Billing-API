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
using System.Threading.Tasks;

namespace BL.Services
{
    public class AuditLogService : BaseService<TbAuditLog, AuditLogDto>, IAuditLog
    {
        private readonly ITableRepository<TbAuditLog> _Repo;
        private readonly IViewRepository<ApplicationUser> _userRepo;

        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public AuditLogService(
            ITableRepository<TbAuditLog> tableRepository,
            IViewRepository<ApplicationUser> viewRepository,
            IMapper mapper,
            IUserService userService)
            : base(tableRepository, mapper, userService)
        {
            _Repo = tableRepository;
            _userRepo = viewRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<List<AuditLogDto>> GetAllAsync()
        {
            var result = await (
                from audit in _Repo.GetTable()

                join user in _userRepo.GetTable()
                    on audit.UserId equals (Guid?)user.Id
                    into userGroup

                from user in userGroup.DefaultIfEmpty()

                orderby audit.Timestamp descending

                select new AuditLogDto
                {
                    Id = audit.Id,
                    UserId = audit.UserId,

                    FirstName = user != null ? user.FirstName : null,
                    LastName = user != null ? user.LastName : null,
                    Email = user != null ? user.Email : null,

                    Action = audit.Action,
                    EntityType = audit.EntityType,
                    EntityId = audit.EntityId,
                    Metadata = audit.Metadata,
                    CorrelationId = audit.CorrelationId,

                    Timestamp = audit.Timestamp
                }
            ).ToListAsync();

            return result;
        }

        public async Task<AuditLogDto?> GetByIdAsync(Guid id)
        {
            var result = await (
                from audit in _Repo.GetTable()

                join user in _userRepo.GetTable()
                    on audit.UserId equals (Guid?)user.Id
                    into userGroup

                from user in userGroup.DefaultIfEmpty()

                where audit.Id == id

                select new AuditLogDto
                {
                    Id = audit.Id,
                    UserId = audit.UserId,

                    FirstName = user != null ? user.FirstName : null,
                    LastName = user != null ? user.LastName : null,
                    Email = user != null ? user.Email : null,

                    Action = audit.Action,
                    EntityType = audit.EntityType,
                    EntityId = audit.EntityId,
                    Metadata = audit.Metadata,
                    CorrelationId = audit.CorrelationId,

                    Timestamp = audit.Timestamp
                }
            ).FirstOrDefaultAsync();

            return result;
        }

        public async Task<Guid> AddAsync(AuditLogDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.Action))
            {
                throw new ArgumentException("Action is required.");
            }

            if (dto.Action.Length > 250)
            {
                throw new ArgumentException(
                    "Action cannot exceed 250 characters.");
            }

            if (!Enum.IsDefined(typeof(EntityType), dto.EntityType))
            {
                throw new ArgumentException(
                    "Invalid EntityType.");
            }

            if (dto.EntityId == Guid.Empty)
            {
                throw new ArgumentException(
                    "EntityId is required.");
            }

            if (dto.Metadata != null && dto.Metadata.Length > 4000)
            {
                throw new ArgumentException(
                    "Metadata cannot exceed 4000 characters.");
            }

            var userId = _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Logged-in user could not be identified.");
            }

            dto.UserId = userId;

            var entity = _mapper.Map<TbAuditLog>(dto);

            entity.Id = Guid.NewGuid();
            entity.Timestamp = DateTime.UtcNow;

        
            entity.CorrelationId =
                (dto.CorrelationId == null || dto.CorrelationId == Guid.Empty)
                    ? Guid.NewGuid()
                    : dto.CorrelationId;

            await _Repo.AddAsync(entity, userId);

            return entity.Id;
        }
    }
}