using System;
using System.Threading.Tasks;
using Idempotency.Core;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Idempotency.Redis
{
    public class IdempotencyRepository : IIdempotencyRepository
    {
        private readonly IDatabase _database;
        private readonly IIdempotencySerializer _serializer;

        public IdempotencyRepository(IDatabase database, IIdempotencySerializer serializer)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public async Task<bool> TryAddAsync(string key)
        {
            if (await _database.KeyExistsAsync((RedisKey) key))
            {
                return false;
            }

            using (var factory = RedLockFactory.Create(new[] {new RedLockMultiplexer(_database.Multiplexer)}))
            using (var redLock = factory.CreateLock(key, TimeSpan.FromMinutes(1)))
            {
                if (!redLock.IsAcquired || await _database.KeyExistsAsync((RedisKey) key))
                {
                    return false;
                }

                var value = _serializer.Serialize(IdempotencyRegister.Of(key));
                return await _database.StringSetAsync(key, value, TimeSpan.FromMinutes(1), When.NotExists);
            }
        }

        public async Task UpdateAsync<T>(string key, T register)
            where T : IIdempotencyRegister
        {
            var value = _serializer.Serialize(register);
            await _database.StringSetAsync(key, value, TimeSpan.FromDays(1), When.Exists);
        }

        public async Task<T> GetAsync<T>(string key)
            where T : IIdempotencyRegister
        {
            var value = await _database.StringGetAsync(key);
            return _serializer.Deserialize<T>(value);
        }

        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}