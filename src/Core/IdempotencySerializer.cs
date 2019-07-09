using System;
using ZeroFormatter;

namespace Idempotency.Core
{
    public class IdemportencySerializer : IIdempotencySerializer
    {
        public string Serialize<T>(T instance)
        {
            return Convert.ToBase64String(ZeroFormatterSerializer.Serialize(instance));
        }

        public T Deserialize<T>(string json)
        {
            return ZeroFormatterSerializer.Deserialize<T>(Convert.FromBase64String(json));
        }
    }
}