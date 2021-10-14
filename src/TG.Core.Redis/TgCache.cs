using System;
using System.Threading.Tasks;
using IRedisCache = StackExchange.Redis.IDatabase;

namespace Flow.Core.Redis
{
    public abstract class TgCache
    {
        private readonly IRedisCache _redisCache;

        protected TgCache(IRedisCache redisCache)
        {
            _redisCache = redisCache;
        }

        protected abstract string CacheKey { get; }
        protected abstract Task ReloadCacheDataAsync();

        protected async Task<TReturn> ExecuteSafe<TReturn>(Func<Task<TReturn>> action)
        {
            if (await _redisCache.KeyExistsAsync(CacheKey))
            {
                return await action.Invoke();
            }

            await ReloadCacheDataAsync();
            return await action.Invoke();
        }

        protected async Task ExecuteSafe(Func<Task> action)
        {
            if (await _redisCache.KeyExistsAsync(CacheKey))
            {
                await action.Invoke();
            }

            await ReloadCacheDataAsync();
            await action.Invoke();
        }
    }
}
