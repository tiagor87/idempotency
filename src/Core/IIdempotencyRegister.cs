namespace Idempotency.Core
{
    public interface IIdempotencyRegister
    {
        string Key { get; }
        bool IsCompleted { get; }
        string Value { get; }
        
    }
}