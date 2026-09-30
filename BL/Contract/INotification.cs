using BL.Dtos;
using Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BL.Contract
{
    public interface INotification : IBaseService<TbNotification, NotificationDto>
    {
        Task<NotificationDto> AddAsync(NotificationDto dto);

        Task<NotificationDto?> GetByIdAsync(Guid id);

        Task<List<NotificationDto>> GetByOrganizationIdAsync(Guid organizationId);

    }
}