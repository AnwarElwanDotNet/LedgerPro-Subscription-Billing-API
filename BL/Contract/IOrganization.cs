using BL.Dtos;
using Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BL.Contract
{
    public interface IOrganization : IBaseService<TbOrganization, OrganizationDto>
    {
        Task<Guid> SaveAsync(OrganizationDto dto);
    }
}