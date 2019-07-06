namespace Idempotency.Core
{
    public interface IIdempotencyKeyReader<TRequest>
    {
        string Read(TRequest request);
    }
}