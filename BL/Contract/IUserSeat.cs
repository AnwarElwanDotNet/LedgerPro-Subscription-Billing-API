using BL.Dtos;
using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Contract
{
    public interface IUserSeat : IBaseService<TbUserSeat,UserSeatDto>
    {
        Task<PagedResult<UserSeatDto>> GetAllAsync(
         int pageNumber,
         int pageSize);

        Task<UserSeatDto?> GetByIdAsync(Guid id);

        Task<UserSeatDto> AddUserSeatAsync(
            Guid organizationId,
            Guid subscriptionId,
            Guid userId);

        Task<UserSeatDto> RemoveUserSeatAsync(
            Guid userSeatId);
    }
}
