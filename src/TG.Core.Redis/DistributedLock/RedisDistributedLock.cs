using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace TG.Core.Redis.DistributedLock
{
    public class RedisDistributedLock : IDistributedLock
    {
        private readonly IDatabase _db;
        private readonly ILogger _logger;
        private readonly Guid _instanceId;

        public RedisDistributedLock(
            IDatabase database,
            ILogger<RedisDistributedLock> logger)
        {
            _db = database;
            _logger = logger;
            _instanceId = Guid.NewGuid();
        }

        public Task<DistributedLockObject> AcquireLockAsync(string lockKey, int tryGetLockDelayMs = 200)
        {
            return AcquireLockAsync(lockKey, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(10), tryGetLockDelayMs);
        }

        public Task<DistributedLockObject> AcquireLockAsync(string lockKey, TimeSpan retryTimeout, int tryGetLockDelayMs = 200)
        {
            return AcquireLockAsync(lockKey, retryTimeout, TimeSpan.FromSeconds(10), tryGetLockDelayMs);
        }

        public async Task<DistributedLockObject> AcquireLockAsync(string lockKey, TimeSpan retryTimeout, TimeSpan lockTimeout, int tryGetLockDelayMs = 200)
        {
            var lockObject = new DistributedLockObject(lockKey, _instanceId, lockTimeout, _logger, _db);
            return await lockObject.LockAsync(retryTimeout, tryGetLockDelayMs);
        }
    }
}