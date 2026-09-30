using BL.Dtos;
using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Contract
{
    public interface ISystemConfig
    {
        Task<SystemConfigDto> GetAsync();

        Task<SystemConfigDto> UpdateAsync(SystemConfigDto dto);
    }
}