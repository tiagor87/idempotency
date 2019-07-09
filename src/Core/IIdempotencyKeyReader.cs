namespace Idempotency.Core
{
    public interface IIdempotencyKeyReader<in TRequest>
    {
        string Read(TRequest request);
    }
}