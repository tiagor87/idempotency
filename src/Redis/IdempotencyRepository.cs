using System;
using System.Threading.Tasks;
using Idempotency.Core;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using ZeroFormatter;

namespace Idempotency.Redis
{
    public class IdempotencyRepository : IIdempotencyRepository
    {
        private readonly IDatabase _database;

        public IdempotencyRepository(IDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<bool> TryAddAsync(string key)
        {
            if (await _database.KeyExistsAsync((RedisKey) key))
            {
                return false;
            }

            using (var factory = RedLockFactory.Create(new[] {new RedLockMultiplexer(_database.Multiplexer)}))
            using (factory.CreateLock(key, TimeSpan.FromMinutes(1)))
            {
                if (await _database.KeyExistsAsync((RedisKey) key))
                {
                    return false;
                }

                var value = Convert.ToBase64String(ZeroFormatterSerializer.Serialize(IdempotencyRegister.Of(key)));
                return await _database.StringSetAsync(key, value, TimeSpan.FromMinutes(1), When.NotExists);
            }
        }

        public async Task UpdateAsync(string key, IdempotencyRegister register)
        {
            var value = Convert.ToBase64String(ZeroFormatterSerializer.Serialize(register));
            await _database.StringSetAsync(key, value, TimeSpan.FromDays(1), When.Exists);
        }

        public async Task<IdempotencyRegister> GetAsync(string key)
        {
            var value = await _database.StringGetAsync(key);
            return ZeroFormatterSerializer.Deserialize<IdempotencyRegister>(Convert.FromBase64String(value));
        }

        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}