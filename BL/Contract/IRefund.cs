using BL.Dtos;
using Domain;
using System;
using System.Collections.Generic;

namespace BL.Contract
{
    public interface IRefund : IBaseService<TbRefund, RefundDto>
    {
        Task ProcessRefundAsync(RefundDto dto);
    }
}
