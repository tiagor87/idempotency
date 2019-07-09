using System;
using System.IO;
using System.Net;
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
            var register = HttpIdempotencyRegister.Of(key);

            register.Should().NotBeNull();
            register.Key.Should().Be(key);
            register.Value.Should().BeNull();
            register.IsCompleted.Should().BeFalse();
            register.StatusCode.Should().Be(0);
        }

        [Trait("Category", "Validation")]
        [Theory(DisplayName =
            "GIVEN IdempotencyRegister, WHEN instantiate with status code and Stream, SHOULD returns instance with key, status code and body set")]
        [InlineData("key", HttpStatusCode.OK, "A body message")]
        [InlineData("another key", HttpStatusCode.Created, "Another body message")]
        [InlineData("just another key 2", HttpStatusCode.Accepted, "Just Another body message")]
        [InlineData("just another key 3", HttpStatusCode.PartialContent, "Just Another body message 2")]
        [InlineData("just another key 4", HttpStatusCode.MultiStatus, "Just Another body message 3")]
        [InlineData("just another key 5", HttpStatusCode.AlreadyReported, "Just Another body message 4")]
        [InlineData("just another key 6", HttpStatusCode.IMUsed, "Just Another body message 5")]
        public void
            GivenIdempotencyRegisterWhenInstantiateWithStatusCodeAndStreamShouldReturnsInstanceWithKeyStatusCodeAndBodySet(
                string key, HttpStatusCode statusCode, string body)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var register = HttpIdempotencyRegister.Of(key, statusCode, stream);

            register.Should().NotBeNull();
            register.Key.Should().Be(key);
            register.IsCompleted.Should().BeTrue();
            register.Value.Should().Be(body);
            register.StatusCode.Should().Be(statusCode);
        }

        [Trait("Category", "Validation")]
        [Theory(DisplayName =
            "GIVEN IdempotencyRegister, WHEN instantiate AND key is null or empty, SHOULD throws ArgumentNullException")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GivenIdempotencyRegisterWhenInstantiateAndKeyIsEmptyShouldThrowsArgumentNullException(string key)
        {
            Func<IIdempotencyRegister> action = () => HttpIdempotencyRegister.Of(key);

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
            Func<IIdempotencyRegister> action = () => HttpIdempotencyRegister.Of(key, HttpStatusCode.OK, new MemoryStream());

            action.Should().Throw<ArgumentNullException>();
        }

        [Trait("Category", "Validation")]
        [Theory(DisplayName =
            "GIVEN IdempotencyRegister, WHEN instantiate with status code and stream AND status code is out of range, SHOULD throws ArgumentOutOfRangeException")]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.PaymentRequired)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.NotAcceptable)]
        [InlineData(HttpStatusCode.ProxyAuthenticationRequired)]
        [InlineData(HttpStatusCode.RequestTimeout)]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.Gone)]
        [InlineData(HttpStatusCode.LengthRequired)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [InlineData(HttpStatusCode.RequestEntityTooLarge)]
        [InlineData(HttpStatusCode.RequestUriTooLong)]
        [InlineData(HttpStatusCode.UnsupportedMediaType)]
        [InlineData(HttpStatusCode.RequestedRangeNotSatisfiable)]
        [InlineData(HttpStatusCode.ExpectationFailed)]
        [InlineData(HttpStatusCode.UnprocessableEntity)]
        [InlineData(HttpStatusCode.Locked)]
        [InlineData(HttpStatusCode.FailedDependency)]
        [InlineData(HttpStatusCode.UpgradeRequired)]
        [InlineData(HttpStatusCode.PreconditionRequired)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.RequestHeaderFieldsTooLarge)]
        [InlineData(HttpStatusCode.UnavailableForLegalReasons)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.NotImplemented)]
        [InlineData(HttpStatusCode.BadGateway)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.GatewayTimeout)]
        [InlineData(HttpStatusCode.HttpVersionNotSupported)]
        [InlineData(HttpStatusCode.VariantAlsoNegotiates)]
        [InlineData(HttpStatusCode.InsufficientStorage)]
        [InlineData(HttpStatusCode.LoopDetected)]
        [InlineData(HttpStatusCode.NotExtended)]
        [InlineData(HttpStatusCode.NetworkAuthenticationRequired)]
        public void
            GivenIdempotencyRegisterWhenInstantiateWithStatusCodeAndStreamAndStatusCodeIsOutRangeShouldThrowsArgumentException(
                HttpStatusCode statusCode)
        {
            Func<IIdempotencyRegister> action = () => HttpIdempotencyRegister.Of("key", statusCode, new MemoryStream());

            action.Should().Throw<ArgumentException>();
        }

        [Trait("Category", "Validation")]
        [Fact(DisplayName =
            "GIVEN IdempotencyRegister, WHEN instantiate with status code and stream AND status code is out of range, SHOULD throws ArgumentOutOfRangeException")]
        public void
            GivenIdempotencyRegisterWhenInstantiateWithStatusCodeAndStreamAndStreamIsNullShouldThrowsArgumentNullException()
        {
            Func<IIdempotencyRegister> action = () => HttpIdempotencyRegister.Of("key", HttpStatusCode.OK, null);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}