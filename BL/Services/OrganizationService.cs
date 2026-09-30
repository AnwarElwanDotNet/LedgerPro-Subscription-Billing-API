
using AutoMapper;
using BL.Contract;
using BL.Dtos;
using BL.Exceptions;
using DAL.Contracts;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BL.Services
{
    public class OrganizationService
        : BaseService<TbOrganization, OrganizationDto>, IOrganization
    {
        private readonly ITableRepository<TbOrganization> _repository;
        private readonly IUserService _userService;

        public OrganizationService(
            ITableRepository<TbOrganization> tableRepository,
            IMapper mapper,
            IUserService userService)
            : base(tableRepository, mapper, userService)
        {
            _repository = tableRepository;
            _userService = userService;
        }

        public async Task<Guid> SaveAsync(OrganizationDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.OrganizationName))
                throw new ArgumentException(
                    "Organization name is required.",
                    nameof(dto.OrganizationName));

            // =========================================================
            // AUTHENTICATION
            // =========================================================

            var currentUserId = _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            var currentUser = await _userService.GetUserByIdAsync(
                currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            // =========================================================
            // AUTHORIZATION
            // Organization management is a Platform Admin operation.
            // =========================================================

            var isPlatformAdmin =
                currentUser.Roles.Any(r =>
                    string.Equals(
                        r,
                        "Platform Admin",
                        StringComparison.OrdinalIgnoreCase));

            if (!isPlatformAdmin)
                throw new ForbiddenAccessException(
                    "Only Platform Admin can manage organizations.");

            // =========================================================
            // NORMALIZE INPUT
            // =========================================================

            dto.OrganizationName = dto.OrganizationName.Trim();

            // =========================================================
            // ADD
            // =========================================================

            if (dto.Id == Guid.Empty)
            {
                return await base.AddAsync(dto);
            }

            // =========================================================
            // UPDATE
            // =========================================================

            var organization = await _repository.GetByIdAsync(dto.Id);

            if (organization == null ||
                organization.CurrentState <= 0)
            {
                throw new KeyNotFoundException(
                    "Organization not found.");
            }

            await base.UpdateAsync(dto);

            return dto.Id;
        }
    }
}

