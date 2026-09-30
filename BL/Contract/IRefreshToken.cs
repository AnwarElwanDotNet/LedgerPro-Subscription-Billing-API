using BL.Dtos;
using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Contract
{
    public interface IRefreshToken : IBaseService<TbRefreshToken, RefreshTokenDto>
    {
        public Task<bool> Refresh(RefreshTokenDto tokenDto);
    }
}
