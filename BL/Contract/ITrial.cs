using BL.Dtos;
using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Contract
{
    public interface ITrial : IBaseService<TbTrial, TrialDto>
    {
        Task<TrialDto> StartTrialAsync(Guid organizationId);

        Task<TrialDto> GetCurrentTrialAsync(Guid organizationId);

        Task<TrialDto> ExpireTrialAsync(Guid trialId);
    }
}
