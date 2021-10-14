using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

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
            services.AddSingleton<IDistributedLockFactory>(p =>
            {
                var redis = ConnectionMultiplexer.Connect(connectionString);
                return RedLockFactory.Create(new List<RedLockMultiplexer> { redis });
            });

            return services;
        }
    }
}
