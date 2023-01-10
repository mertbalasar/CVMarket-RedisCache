﻿using CVMarket.Business.Interfaces;
using CVMarket.Core.Interfaces;
using CVMarket.Core.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CVMarket.Business.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IHttpContextAccessor _httpAccessor;

        public CacheService(IDistributedCache cache, IHttpContextAccessor httpAccessor)
        {
            _cache = cache;
            _httpAccessor = httpAccessor;
        }

        public async Task<ServiceResponse<TOut>> GetCache<TOut>(IRequestBase request)
        {
            var response = new ServiceResponse<TOut> { };

            try
            {
                var key = KeyGenerator(request);

                if (key.Code != 200)
                {
                    response.Code = key.Code;
                    response.Message = key.Message;
                    goto exit;
                }

                var json = await _cache.GetStringAsync(key.Result);
                if (!string.IsNullOrEmpty(json))
                {
                    TOut obj = JsonConvert.DeserializeObject<TOut>(json);
                    response.Result = obj;
                }
            }
            catch (Exception e)
            {
                response.Code = 500;
                response.Message = e.Message;
            }

            exit:;
            return response;
        }

        public async Task<ServiceResponse<bool>> SetCache<TOut>(IRequestBase request, TOut value)
        {
            var response = new ServiceResponse<bool> { };

            try
            {
                var key = KeyGenerator(request);

                if (key.Code != 200)
                {
                    response.Code = key.Code;
                    response.Message = key.Message;
                    goto exit;
                }

                var options = new DistributedCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromDays(1))
                        .SetAbsoluteExpiration(DateTime.Now.AddDays(5));
                var json = JsonConvert.SerializeObject(value);
                await _cache.SetStringAsync(key.Result, json, options);

                response.Result = true;
            }
            catch (Exception e)
            {
                response.Code = 500;
                response.Message = e.Message;
            }

            exit:;
            return response;
        }

        private ServiceResponse<string> KeyGenerator(IRequestBase request)
        {
            var response = new ServiceResponse<string> { };

            try
            {
                var result = JsonConvert.SerializeObject((_httpAccessor.HttpContext.Request.Path, request));
                response.Result = result;
            }
            catch (Exception e)
            {
                response.Code = 500;
                response.Message = e.Message;
            }

            return response;
        }
    }
}
