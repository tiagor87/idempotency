using System;
using System.Threading.Tasks;
using Idempotency.Core;
using ServiceStack.Redis;

namespace Idempotency.Redis
{
    public class IdempotencyRepository : IIdempotencyRepository
    {
        private readonly IRedisClient _redisClient;

        public IdempotencyRepository(IRedisClient redisClient)
        {
            _redisClient = redisClient ?? throw new ArgumentNullException(nameof(redisClient));
        }

        public Task<bool> TryAddAsync(string key)
        {
            return Task.Factory.StartNew(() =>
            {
                var register = IdempotencyRegister.Of(key);
                var data = Convert.ToBase64String(ZeroFormatter.ZeroFormatterSerializer.Serialize(register));
                var created = _redisClient.SetValueIfNotExists(key, data);
                return created && _redisClient.ExpireEntryIn(key, TimeSpan.FromSeconds(60));
            });
        }

        public Task UpdateAsync(string key, IdempotencyRegister register)
        {
            return Task.Factory.StartNew(() =>
            {
                var data = Convert.ToBase64String(ZeroFormatter.ZeroFormatterSerializer.Serialize(register));
                _redisClient.SetValue(key, data, TimeSpan.FromDays(1));
            });
        }

        public Task<IdempotencyRegister> GetAsync(string key)
        {
            return Task.Factory.StartNew(() =>
            {
                var data = Convert.FromBase64String(_redisClient.GetValue(key));
                return data.Length == 0
                    ? null
                    : ZeroFormatter.ZeroFormatterSerializer.Deserialize<IdempotencyRegister>(data);
            });
        }

        public Task RemoveAsync(string key)
        {
            return Task.Factory.StartNew(() => _redisClient.Remove(key));
        }
    }
}