using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Contract
{
    public interface IBaseService<T, DTO>
    {
        Task<DTO?> GetByIdAsync(Guid id);
        Task<List<DTO>> GetAllAsync();
        Task<Guid> AddAsync(DTO dto);
        Task UpdateAsync(DTO dto);
        Task DeleteAsync(Guid id);
        Task<bool> ChangeStatusAsync(Guid id, RecordStatus recordStatus = RecordStatus.Deleted);
    }
}
