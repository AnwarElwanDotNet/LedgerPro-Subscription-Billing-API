using AutoMapper;
using BL.Contract;
using BL.Dtos;
using DAL.Contracts;
using Domain;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Services
{
    public class RefreshTokenService : BaseService<TbRefreshToken, RefreshTokenDto>, IRefreshToken
    {
        ITableRepository<TbRefreshToken> _repo;
        IMapper _mapper;
        private readonly IUserService _userService;
        public RefreshTokenService(ITableRepository<TbRefreshToken> repo, IMapper mapper,
            IUserService userService) : base(repo, mapper, userService)
        {
            _repo = repo;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<bool> Refresh(RefreshTokenDto tokenDto)
        {
            var tokenList = await _repo.GetListAsync(a => a.UserId == Guid.Parse(tokenDto.UserId) && a.CurrentState == 1);
            foreach (var dbToken in tokenList)
            {
                await _repo.ChangeStatusAsync(dbToken.Id, Guid.Parse(tokenDto.UserId), RecordStatus.Active);
            }

            var dbTokens = _mapper.Map<RefreshTokenDto, TbRefreshToken>(tokenDto);
            var userId = Guid.Parse(tokenDto.UserId);
            await _repo.AddAsync(dbTokens, userId);
            return true;
        }
    }

}
