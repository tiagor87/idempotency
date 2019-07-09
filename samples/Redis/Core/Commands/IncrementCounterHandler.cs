using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Idempotency.Samples.Redis.Core.Commands
{
    public class IncrementCounterHandler : IRequestHandler<IncrementCounter, int>
    {
        private static int _counter;
        public Task<int> Handle(IncrementCounter request, CancellationToken cancellationToken)
        {
            var counter = 0;
            for (var i = 0; i < request.Count; i++)
            {
                counter = Interlocked.Increment(ref _counter);
            }
            return Task.FromResult(counter);
        }
    }
}