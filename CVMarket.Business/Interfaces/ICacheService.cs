using CVMarket.Core.Interfaces;
using CVMarket.Core.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CVMarket.Business.Interfaces
{
    public interface ICacheService
    {
        Task<ServiceResponse<TOut>> GetCache<TOut>(IRequestBase request);
        Task<ServiceResponse<bool>> SetCache<TOut>(IRequestBase request, TOut value);
    }
}
