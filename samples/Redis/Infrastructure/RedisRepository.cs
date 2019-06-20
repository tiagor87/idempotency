using System;
using System.Threading.Tasks;
using Idempotency.Core;
using Newtonsoft.Json;
using ServiceStack.Redis;

namespace Idempotency.Redis.Infrastructure
{
    public class RedisRepository : IIdempotencyRepository
    {
        private readonly IRedisClient _redisClient;

        public RedisRepository(IRedisClient redisClient)
        {
            _redisClient = redisClient ?? throw new ArgumentNullException(nameof(redisClient));
        }

        public Task<bool> TryAddAsync(string key)
        {
            return Task.Factory.StartNew(() =>
            {
                var register = IdempotencyRegister.Of(key);
                var created = _redisClient.SetValueIfNotExists(key, JsonConvert.SerializeObject(register));
                return created
                    ? _redisClient.ExpireEntryIn(key, TimeSpan.FromSeconds(60))
                    : created;
            });
        }

        public Task UpdateAsync(string key, IdempotencyRegister register)
        {
            return Task.Factory.StartNew(() =>
            {
                _redisClient.SetValue(key, JsonConvert.SerializeObject(register), TimeSpan.FromDays(1));
            });
        }

        public Task<IdempotencyRegister> GetAsync(string key)
        {
            return Task.Factory.StartNew(() =>
            {
                var json = _redisClient.GetValue(key);
                return string.IsNullOrWhiteSpace(json)
                    ? null
                    : JsonConvert.DeserializeObject<IdempotencyRegister>(json);
            });
        }

        public Task RemoveAsync(string key)
        {
            return Task.Factory.StartNew(() => { _redisClient.Remove(key); });
        }
    }
}