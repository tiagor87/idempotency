using Microsoft.AspNetCore.Http;

namespace Idempotency.Core
{
    public interface IIdempotencyKeyReader
    {
        string Read(HttpRequest request);
    }
}