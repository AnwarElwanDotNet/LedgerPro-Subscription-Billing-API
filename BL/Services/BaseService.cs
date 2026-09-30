using AutoMapper;
using BL.Contract;
using DAL.Contracts;
using Domain;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Services
{
    public class BaseService<T, DTO> : IBaseService<T, DTO> where T : BaseTable
    {
        private readonly ITableRepository<T> _repo;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public BaseService(ITableRepository<T> repo, IMapper mapper, IUserService userService)
        {
            _repo = repo;
            _mapper = mapper;
            _userService = userService;
        }

        public int AddedItemsCount { get; private set; } = 0;

        public async Task<Guid> AddAsync(DTO dto)
        {
            var dbObject = _mapper.Map<DTO, T>(dto);

            dbObject.Id = Guid.NewGuid();
            dbObject.CurrentState = 1;
            dbObject.CreatedDate = DateTime.UtcNow;
            var userId = _userService.GetLoggedInUser();
            await _repo.AddAsync(dbObject, userId);
            AddedItemsCount++;
            return dbObject.Id;
        }

        public async Task<bool> ChangeStatusAsync(Guid id, RecordStatus recordStatus = RecordStatus.Deleted)
        {
            var userId = _userService.GetLoggedInUser();
            try
            {
                await _repo.ChangeStatusAsync(id, userId, recordStatus);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task DeleteAsync(Guid id)
        {

            await _repo.DeleteAsync(id);
        }

        public async Task<List<DTO>> GetAllAsync()
        {
            var dbObject = await _repo.GetAllAsync();

            dbObject = dbObject
                .Where(x => x.CurrentState > 0)
                .ToList();

            return _mapper.Map<List<T>, List<DTO>>(dbObject);
        }

        public virtual async Task<DTO?> GetByIdAsync(Guid id)
        {
            var dbObject = await _repo.GetByIdAsync(id);

            if (dbObject == null)
                return default;

            return _mapper.Map<T, DTO>(dbObject);
        }

        public async Task UpdateAsync(DTO dto)
        {
            var entity = _mapper.Map<T>(dto);

            entity.UpdatedDate = DateTime.UtcNow;
            entity.CurrentState = Math.Max(entity.CurrentState, 1);
            var user = _userService.GetLoggedInUser();
            await _repo.UpdateAsync(entity, user);
        }
    }
}