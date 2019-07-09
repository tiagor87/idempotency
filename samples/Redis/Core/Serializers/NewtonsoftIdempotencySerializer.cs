using Idempotency.Core;
using Newtonsoft.Json;

namespace Idempotency.Samples.Redis.Core.Serializers
{
    public class NewtonsoftIdempotencySerializer: IIdempotencySerializer
    {
        public string Serialize<T>(T instance)
        {
            return JsonConvert.SerializeObject(instance);
        }

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}