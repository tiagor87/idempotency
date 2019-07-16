using System.IO;
using System.Net;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Idempotency.Core.UnitTests
{
    public class IdempotencySerializerTests
    {
        public IdempotencySerializerTests()
        {
            _serializer = new IdempotencySerializer();
        }

        private readonly IIdempotencySerializer _serializer;

        [Fact(DisplayName = "GIVEN HTTP Idempotency Serializer, SHOULD deserialize IdempotencyRegister")]
        public void GivenHttpIdempotencySerializerShouldDeserializeIdempotencyRegister()
        {
            var expectedRegister = HttpIdempotencyRegister.Of("key", HttpStatusCode.OK,
                new MemoryStream(Encoding.UTF8.GetBytes("body")));
            var serializedObject = _serializer.Serialize(expectedRegister);

            var register = _serializer.Deserialize<HttpIdempotencyRegister>(serializedObject);

            register.Key.Should().Be(expectedRegister.Key);
            register.IsCompleted.Should().Be(expectedRegister.IsCompleted);
            register.StatusCode.Should().Be(HttpStatusCode.OK);
            register.Value.Should().Be(expectedRegister.Value);
        }

        [Fact(DisplayName = "GIVEN HTTP Idempotency Serializer, SHOULD serialize IdempotencyRegister")]
        public void GivenHttpIdempotencySerializerShouldSerializeIdempotencyRegister()
        {
            var register = HttpIdempotencyRegister.Of("key", HttpStatusCode.OK,
                new MemoryStream(Encoding.UTF8.GetBytes("body")));

            var serializedObject = _serializer.Serialize(register);

            serializedObject.Should().NotBeNullOrWhiteSpace();
        }

        [Fact(DisplayName = "GIVEN Idempotency Serializer, SHOULD deserialize IdempotencyRegister")]
        public void GivenIdempotencySerializerShouldDeserializeIdempotencyRegister()
        {
            var expectedRegister = IdempotencyRegister.Of("key", "body");
            var serializedObject = _serializer.Serialize(expectedRegister);

            var register = _serializer.Deserialize<IdempotencyRegister>(serializedObject);

            register.Key.Should().Be(expectedRegister.Key);
            register.IsCompleted.Should().Be(expectedRegister.IsCompleted);
            register.Value.Should().Be(expectedRegister.Value);
        }

        [Fact(DisplayName = "GIVEN Idempotency Serializer, SHOULD serialize IdempotencyRegister")]
        public void GivenIdempotencySerializerShouldSerializeIdempotencyRegister()
        {
            var register = IdempotencyRegister.Of("key", "body");

            var serializedObject = _serializer.Serialize(register);

            serializedObject.Should().NotBeNullOrWhiteSpace();
        }
    }
}