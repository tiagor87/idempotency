using System.IO;
using System.Threading.Tasks;

namespace Idempotency.Core
{
    public interface IIdempotencyRepository
    {
        Task<bool> TryAddAsync(string key);
        Task UpdateAsync(string key, Stream body);
        Task<IdempotencyRegister> GetAsync(string key);
        Task RemoveAsync(string key);
    }
}