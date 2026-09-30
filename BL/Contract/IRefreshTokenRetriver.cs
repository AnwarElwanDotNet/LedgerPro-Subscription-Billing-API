using BL.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Contract
{
    public interface IRefreshTokenRetriver
    {
        public Task<RefreshTokenDto> GetByToken(string token);
    }
}
