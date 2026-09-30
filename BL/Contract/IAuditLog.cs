using BL.Dtos;
using Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BL.Contract
{
    public interface IAuditLog : IBaseService<TbAuditLog, AuditLogDto>
    {
        Task<AuditLogDto?> GetByIdAsync(Guid id);

        Task<List<AuditLogDto>> GetAllAsync();

        Task<Guid> AddAsync(AuditLogDto dto);
    }
}