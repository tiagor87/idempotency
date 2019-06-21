using System;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Idempotency.Core.UnitTests
{
    public class IdempotencyKeyReaderTests
    {
        private readonly IdempotencyKeyReader _keyReader;
        private readonly Mock<HttpRequest> _requestMock;

        public IdempotencyKeyReaderTests()
        {
            _requestMock = new Mock<HttpRequest>();
            _keyReader = new IdempotencyKeyReader();
        }

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
    }
}