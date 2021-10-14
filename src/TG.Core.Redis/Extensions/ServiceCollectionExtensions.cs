using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TG.Core.Redis.DistributedLock;

namespace TG.Core.Redis.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Redis Cache services and services that allows to create Distributed locks on Redis cache.<para></para>
        /// Use IDatabase for accessing Redis and IDistributedLockFactory if you need distributed locking of data in Redis.
        /// </summary>
        public static IServiceCollection AddTgRedis(this IServiceCollection services, IConfiguration configuration) =>
            AddTgRedis(services, configuration.GetConnectionString("Redis"));

        /// <summary>
        /// Adds Redis Cache services and services that allows to create Distributed locks on Redis cache.<para></para>
        /// Use IDatabase for accessing Redis and IDistributedLockFactory if you need distributed locking of data in Redis.
        /// </summary>
        public static IServiceCollection AddTgRedis(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IDatabase>(p =>
            {
                var redis = ConnectionMultiplexer.Connect(connectionString);
                return redis.GetDatabase();
            });
            services.AddTransient<IDistributedLock, RedisDistributedLock>(p =>
            {
                var redis = ConnectionMultiplexer.Connect(connectionString);
                return new RedisDistributedLock(redis.GetDatabase(), p.GetRequiredService<ILogger<RedisDistributedLock>>());
            });

            return services;
        }
    }
}
