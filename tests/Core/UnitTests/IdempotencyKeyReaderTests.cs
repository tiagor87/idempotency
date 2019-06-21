using System;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Idempotency.Core.UnitTests
{
    public class IdempotencyKeyReaderTests
    {
        public IdempotencyKeyReaderTests()
        {
            _requestMock = new Mock<HttpRequest>();
            _keyReader = new IdempotencyKeyReader();
        }

        private readonly IdempotencyKeyReader _keyReader;
        private readonly Mock<HttpRequest> _requestMock;

        [Trait("Category", "Cases")]
        [Theory(DisplayName = @"GIVEN Request, WHEN Idempotency-Key is avaiable, SHOULD concat Request Method and Key")]
        [InlineData(nameof(HttpMethods.Post))]
        [InlineData(nameof(HttpMethods.Put))]
        [InlineData(nameof(HttpMethods.Patch))]
        [InlineData(nameof(HttpMethods.Delete))]
        public void GivenRequestWhenIdempotencyKeyAvaiableShouldConcatMethodAndKey(string method)
        {
            var key = Guid.NewGuid().ToString();
            _requestMock.SetupGet(x => x.Headers.Keys)
                .Returns(new[] {IdempotencyKeyReader.IDEMPOTENCY_KEY})
                .Verifiable();
            _requestMock.SetupGet(x => x.Headers[IdempotencyKeyReader.IDEMPOTENCY_KEY])
                .Returns(key)
                .Verifiable();
            _requestMock.SetupGet(x => x.Method)
                .Returns(method)
                .Verifiable();

            var idempotencyKey = _keyReader.Read(_requestMock.Object);

            idempotencyKey.Should()
                .Contain(method)
                .And.Contain(key);
            _requestMock.VerifyAll();
        }

        [Trait("Category", "Cases")]
        [Theory(DisplayName = @"GIVEN Request, WHEN Idempotency-Key is avaiable, SHOULD search as case insensitive")]
        [InlineData("idempotency-key")]
        [InlineData("Idempotency-Key")]
        [InlineData("iDemPotEncY-kEy")]
        public void GivenRequestWhenIdempotencyKeyAvaiableShouldSearchAsCaseInsensitive(string key)
        {
            _requestMock.SetupGet(x => x.Headers.Keys)
                .Returns(new[] {IdempotencyKeyReader.IDEMPOTENCY_KEY})
                .Verifiable();
            _requestMock.SetupGet(x => x.Headers[IdempotencyKeyReader.IDEMPOTENCY_KEY])
                .Returns(key)
                .Verifiable();
            _requestMock.SetupGet(x => x.Method)
                .Returns(HttpMethods.Post)
                .Verifiable();

            var idempotencyKey = _keyReader.Read(_requestMock.Object);

            idempotencyKey.Should()
                .NotBeNullOrWhiteSpace();
            _requestMock.VerifyAll();
        }

        [Trait("Category", "Cases")]
        [Theory(DisplayName =
            "GIVEN Request, WHEN Idempotency-Key is avaiable AND valus is empty, SHOULD returns null")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GivenRequestWhenIdempotencyKeyAvaiableAndValueIsEmptyShouldReturnsNull(string value)
        {
            _requestMock.SetupGet(x => x.Headers.Keys)
                .Returns(new string[] {IdempotencyKeyReader.IDEMPOTENCY_KEY})
                .Verifiable();
            _requestMock.SetupGet(x => x.Headers[IdempotencyKeyReader.IDEMPOTENCY_KEY])
                .Returns(value)
                .Verifiable();

            var idempotencyKey = _keyReader.Read(_requestMock.Object);

            idempotencyKey.Should()
                .BeNull();
            _requestMock.VerifyAll();
        }

        [Trait("Category", "Cases")]
        [Fact(DisplayName = "GIVEN Request, WHEN Idempotency-Key isn't avaiable, SHOULD returns null")]
        public void GivenRequestWhenIdempotencyKeyIsntAvaiableShouldReturnsNull()
        {
            _requestMock.SetupGet(x => x.Headers.Keys)
                .Returns(new string[] { })
                .Verifiable();

            var idempotencyKey = _keyReader.Read(_requestMock.Object);

            idempotencyKey.Should()
                .BeNull();
            _requestMock.VerifyAll();
        }
    }
}