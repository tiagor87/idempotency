namespace Idempotency.Core
{
    public interface IIdempotencySerializer
    {
        string Serialize<T>(T instance);
        T Deserialize<T>(string json);
    }
}