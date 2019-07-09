using Idempotency.Core;

namespace Idempotency.Samples.Redis.Core.Commands
{
    public class IncrementCounterKeyReader : IIdempotencyKeyReader<IncrementCounter>

    {
        public string Read(IncrementCounter request)
        {
            return request.Key;
        }
    }
}