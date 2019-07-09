using System;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Idempotency.Core.UnitTests
{
    public class HttpRequestIdempotencyKeyReaderTests
    {
        public HttpRequestIdempotencyKeyReaderTests()
        {
            _requestMock = new Mock<HttpRequest>();
            _keyReader = new HttpRequestIdempotencyKeyReader();
        }

        private readonly HttpRequestIdempotencyKeyReader _keyReader;
        private readonly Mock<HttpRequest> _requestMock;

        [Trait("Category", "Cases")]
        [Theory(DisplayName =
            @"GIVEN Request, WHEN Idempotency-Key is avaiable, SHOULD concat request path, method and Key")]
        [InlineData("/path/to/post", nameof(HttpMethods.Post))]
        [InlineData("/path/to/put", nameof(HttpMethods.Put))]
        [InlineData("/path/to/patch", nameof(HttpMethods.Patch))]
        [InlineData("/path/to/delete", nameof(HttpMethods.Delete))]
        public void GivenRequestWhenIdempotencyKeyAvaiableShouldConcatMethodAndKey(string path, string method)
        {
            var key = Guid.NewGuid().ToString();
            _requestMock.SetupGet(x => x.Headers.Keys)
                .Returns(new[] {HttpRequestIdempotencyKeyReader.IDEMPOTENCY_KEY})
                .Verifiable();
            _requestMock.SetupGet(x => x.Headers[HttpRequestIdempotencyKeyReader.IDEMPOTENCY_KEY])
                .Returns(key)
                .Verifiable();
            _requestMock.SetupGet(x => x.Path)
                .Returns(path)
                .Verifiable();
            _requestMock.SetupGet(x => x.Method)
                .Returns(method)
                .Verifiable();

            var idempotencyKey = _keyReader.Read(_requestMock.Object);

            idempotencyKey.Should()
                .Contain(path)
                .And.Contain(method)
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
                .Returns(new[] {HttpRequestIdempotencyKeyReader.IDEMPOTENCY_KEY})
                .Verifiable();
            _requestMock.SetupGet(x => x.Headers[HttpRequestIdempotencyKeyReader.IDEMPOTENCY_KEY])
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
                .Returns(new string[] {HttpRequestIdempotencyKeyReader.IDEMPOTENCY_KEY})
                .Verifiable();
            _requestMock.SetupGet(x => x.Headers[HttpRequestIdempotencyKeyReader.IDEMPOTENCY_KEY])
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