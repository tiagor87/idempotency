using MediatR;

namespace Idempotency.Samples.Redis.Core.Commands
{
    public class IncrementCounter : IRequest<int>
    {
        public string Key { get; }
        public int Count { get; }

        public IncrementCounter(string key, int count = 1)
        {
            Key = key;
            Count = count;
        }
    }
}