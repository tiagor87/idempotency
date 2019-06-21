using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Idempotency.Core.UnitTests
{
    public class IdempotencyRegisterTests
    {
        [Trait("Category", "Validation")]
        [Theory(DisplayName = "GIVEN IdempotencyRegister, WHEN instantiate, SHOULD returns instance with key set")]
        [InlineData("key")]
        [InlineData("another key")]
        [InlineData("just another key")]
        public void GivenIdempotencyRegisterWhenInstantiateShouldReturnsInstanceWithKeySet(string key)
        {
            var register = IdempotencyRegister.Of(key);

            register.Should().NotBeNull();
            register.Key.Should().Be(key);
            register.Body.Should().BeNull();
            register.StatusCode.Should().BeNull();
        }

        [Trait("Category", "Validation")]
        [Theory(DisplayName =
            "GIVEN IdempotencyRegister, WHEN instantiate with status code and Stream, SHOULD returns instance with key, status code and body set")]
        [InlineData("key", 200, "A body message")]
        [InlineData("another key", 201, "Another body message")]
        public void
            GivenIdempotencyRegisterWhenInstantiateWithStatusCodeAndStreamShouldReturnsInstanceWithKeyStatusCodeAndBodySet(
                string key, int statusCode, string body)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var register = IdempotencyRegister.Of(key, statusCode, stream);

            register.Should().NotBeNull();
            register.Key.Should().Be(key);
            register.StatusCode.Should().Be(statusCode);
            register.Body.Should().Be(body);
        }

        [Trait("Category", "Validation")]
        [Theory(DisplayName =
            "GIVEN IdempotencyRegister, WHEN instantiate AND key is null or empty, SHOULD throws ArgumentNullException")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GivenIdempotencyRegisterWhenInstantiateAndKeyIsEmptyShouldThrowsArgumentNullException(string key)
        {
            Func<IdempotencyRegister> action = () => IdempotencyRegister.Of(key);

            action.Should().Throw<ArgumentNullException>();
        }

        [Trait("Category", "Validation")]
        [Theory(DisplayName =
            "GIVEN IdempotencyRegister, WHEN instantiate with status code and stream AND key is null or empty, SHOULD throws ArgumentNullException")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void
            GivenIdempotencyRegisterWhenInstantiateWithStatusCodeAndStreamAndKeyIsEmptyShouldThrowsArgumentNullException(
                string key)
        {
            Func<IdempotencyRegister> action = () => IdempotencyRegister.Of(key, 200, new MemoryStream());

            action.Should().Throw<ArgumentNullException>();
        }

        [Trait("Category", "Validation")]
        [Theory(DisplayName =
            "GIVEN IdempotencyRegister, WHEN instantiate with status code and stream AND status code is out of range, SHOULD throws ArgumentOutOfRangeException")]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(-100)]
        public void
            GivenIdempotencyRegisterWhenInstantiateWithStatusCodeAndStreamAndStatusCodeIsOutRangeShouldThrowsArgumentOutOfRangeException(
                int statusCode)
        {
            Func<IdempotencyRegister> action = () => IdempotencyRegister.Of("key", statusCode, new MemoryStream());

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Trait("Category", "Validation")]
        [Fact(DisplayName =
            "GIVEN IdempotencyRegister, WHEN instantiate with status code and stream AND status code is out of range, SHOULD throws ArgumentOutOfRangeException")]
        public void
            GivenIdempotencyRegisterWhenInstantiateWithStatusCodeAndStreamAndStreamIsNullShouldThrowsArgumentNullException()
        {
            Func<IdempotencyRegister> action = () => IdempotencyRegister.Of("key", 200, null);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}