using System.Threading.Tasks;

namespace Idempotency.Core
{
    public interface IIdempotencyRepository
    {
        Task<bool> TryAddAsync(string key);

        Task UpdateAsync<T>(string key, T register)
            where T : IIdempotencyRegister;

        Task<T> GetAsync<T>(string key)
            where T : IIdempotencyRegister;

        Task RemoveAsync(string key);
    }
}