using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace TG.Core.Redis.DistributedLock
{
    public class DistributedLockObject : IDisposable, IAsyncDisposable
    {
        private readonly string _lockKey;
        private readonly Guid _instanceId;
        private readonly TimeSpan _timeout;
        private readonly IDatabase _db;
        private readonly ILogger _logger;
        private readonly Random _random = new();

        internal DistributedLockObject(string lockKey, Guid instanceId, TimeSpan timeout, ILogger logger, IDatabase db)
        {
            _lockKey = lockKey;
            _instanceId = instanceId;
            _timeout = timeout;
            _logger = logger;
            _db = db;
        }

        internal async Task<DistributedLockObject> LockAsync(TimeSpan retryTimeout, int tryGetLockDelayMs)
        {
            var minDelay = tryGetLockDelayMs / 2;
            var maxDelay = tryGetLockDelayMs * 3 / 2;

            DateTime lockWaitStart = DateTime.UtcNow;
            while (DateTime.UtcNow - lockWaitStart < retryTimeout)
            {
                if (await TryToGetLockAsync())
                {
                    return this;
                }

                var delay = _random.Next(minDelay, maxDelay + 1);
                await Task.Delay(TimeSpan.FromMilliseconds(delay));
            }
            throw new TimeoutException($"Exceeded timeout of {retryTimeout}");
        }


        private async Task<bool> TryToGetLockAsync()
        {
            try
            {
                return await _db.LockTakeAsync(_lockKey, _instanceId.ToString(), _timeout);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting lock");
                return false;
            }
        }
        public void Dispose()
        {
            try
            {
                _db.LockRelease(_lockKey, _instanceId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting lock");
            }
        }
        
        public async ValueTask DisposeAsync()
        {
            try
            {
                await _db.LockReleaseAsync(_lockKey, _instanceId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting lock");
            }
        }
    }
}