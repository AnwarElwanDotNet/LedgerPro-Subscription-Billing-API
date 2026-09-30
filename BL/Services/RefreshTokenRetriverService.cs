using AutoMapper;
using BL.Contract;
using BL.Dtos;
using DAL.Contracts;
using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Services
{
    public class RefreshTokenRetriverService : IRefreshTokenRetriver
    {
        ITableRepository<TbRefreshToken> _repo;
        IMapper _mapper;
        public RefreshTokenRetriverService(ITableRepository<TbRefreshToken> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        public async Task<RefreshTokenDto> GetByToken(string token)
        {
            var refreshToken = await _repo.GetFirstOrDefaultAsync(a => a.Token == token);
            return _mapper.Map<TbRefreshToken, RefreshTokenDto>(refreshToken);
        }
    }
}
