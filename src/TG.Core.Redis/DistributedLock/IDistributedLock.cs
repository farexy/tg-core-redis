using System;
using System.Threading.Tasks;

namespace TG.Core.Redis.DistributedLock
{
    public interface IDistributedLock
    {
        Task<DistributedLockObject> AcquireLockAsync(string lockKey, int tryGetLockDelayMs = 200);
        Task<DistributedLockObject> AcquireLockAsync(string lockKey, TimeSpan retryTimeout, int tryGetLockDelayMs = 200);
        Task<DistributedLockObject> AcquireLockAsync(string lockKey, TimeSpan retryTimeout, TimeSpan lockTimeout, int tryGetLockDelayMs = 200);
    }
}