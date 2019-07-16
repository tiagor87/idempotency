using System.Net.Http.Formatting;
using Idempotency.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Idempotency.Samples.Redis.Core.Serializers
{
    public class NewtonsoftIdempotencySerializer: IIdempotencySerializer
    {
        private readonly JsonSerializerSettings _settings;

        public NewtonsoftIdempotencySerializer()
        {
            _settings = new JsonSerializerSettings()
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ContractResolver = new ContractResolverWithPrivates()
            };
        }
        public string Serialize<T>(T instance)
        {
            return JsonConvert.SerializeObject(instance, _settings);
        }

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }
    }
    
    public class ContractResolverWithPrivates : CamelCasePropertyNamesContractResolver
    {
        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (!prop.Writable)
            {
                var property = member as System.Reflection.PropertyInfo;
                if (property != null)
                {
                    var hasPrivateSetter = property.GetSetMethod(true) != null;
                    prop.Writable = hasPrivateSetter;
                }
            }

            return prop;
        }
    }

}