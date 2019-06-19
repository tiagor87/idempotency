using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Idempotency.Core.UnitTests
{
    public class IdempotencyMiddlewareTests
    {
        public IdempotencyMiddlewareTests()
        {
            _nextMock = new Mock<RequestDelegate>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger>();
            _repositoryMock = new Mock<IIdempotencyRepository>();
            _middleware = new IdempotencyMiddleware(_nextMock.Object, _repositoryMock.Object, _loggerMock.Object);
        }

        private readonly IdempotencyMiddleware _middleware;
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IIdempotencyRepository> _repositoryMock;

        [Trait("Category", "Cases")]
        [Theory(DisplayName =
            @"GIVEN Request, WHEN Idempotency-Key is avaiable AND execution fails, SHOULD remove key")]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.BadGateway)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.PreconditionRequired)]
        [InlineData(HttpStatusCode.RequestTimeout)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        public async Task GivenRequestWhenIdempotencyKeyAvaiableAndExecutionFailsShouldRemoveKey(
            HttpStatusCode statusCode)
        {
            var idempotencyKey = Guid.NewGuid().ToString();
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(x => x.Request.Headers.ContainsKey(IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY))
                .Returns(true)
                .Verifiable();
            contextMock.SetupGet(x => x.Request.Headers[IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY])
                .Returns(idempotencyKey)
                .Verifiable();
            contextMock.SetupGet(x => x.Response.StatusCode)
                .Returns((int) statusCode)
                .Verifiable();
            _repositoryMock.Setup(x => x.TryAddAsync(idempotencyKey))
                .ReturnsAsync(true)
                .Verifiable();
            _repositoryMock.Setup(x => x.RemoveAsync(idempotencyKey))
                .Returns(Task.CompletedTask)
                .Verifiable();
            _nextMock.Setup(x => x.Invoke(contextMock.Object))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await _middleware.InvokeAsync(contextMock.Object);

            contextMock.VerifyAll();
            _repositoryMock.VerifyAll();
            _nextMock.VerifyAll();
        }

        [Trait("Category", "Cases")]
        [Theory(DisplayName =
            @"GIVEN Request, WHEN Idempotency-Key is avaiable AND content type isn't json, SHOULD remove key")]
        [InlineData(MediaTypeNames.Application.Octet)]
        [InlineData(MediaTypeNames.Application.Pdf)]
        [InlineData(MediaTypeNames.Application.Rtf)]
        [InlineData(MediaTypeNames.Application.Soap)]
        [InlineData(MediaTypeNames.Application.Xml)]
        [InlineData(MediaTypeNames.Application.Zip)]
        [InlineData(MediaTypeNames.Image.Gif)]
        [InlineData(MediaTypeNames.Image.Jpeg)]
        [InlineData(MediaTypeNames.Image.Tiff)]
        [InlineData(MediaTypeNames.Text.Html)]
        [InlineData(MediaTypeNames.Text.Plain)]
        [InlineData(MediaTypeNames.Text.Xml)]
        [InlineData(MediaTypeNames.Text.RichText)]
        public async Task GivenRequestWhenIdempotencyKeyAvaiableAndContentTypeIsntJSONShouldRemoveKey(string mediaType)
        {
            var idempotencyKey = Guid.NewGuid().ToString();
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(x => x.Request.Headers.ContainsKey(IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY))
                .Returns(true)
                .Verifiable();
            contextMock.SetupGet(x => x.Request.Headers[IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY])
                .Returns(idempotencyKey)
                .Verifiable();
            contextMock.SetupGet(x => x.Response.StatusCode)
                .Returns((int) HttpStatusCode.OK)
                .Verifiable();
            contextMock.SetupGet(x => x.Response.ContentType)
                .Returns(mediaType)
                .Verifiable();
            _repositoryMock.Setup(x => x.TryAddAsync(idempotencyKey))
                .ReturnsAsync(true)
                .Verifiable();
            _repositoryMock.Setup(x => x.RemoveAsync(idempotencyKey))
                .Returns(Task.CompletedTask)
                .Verifiable();
            _nextMock.Setup(x => x.Invoke(contextMock.Object))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await _middleware.InvokeAsync(contextMock.Object);

            contextMock.VerifyAll();
            _repositoryMock.VerifyAll();
            _nextMock.VerifyAll();
        }

        [Trait("Category", "Logging")]
        [Fact(DisplayName = "GIVEN Request, WHEN conflict, SHOULD log key detected, AND conflict")]
        public async Task GivenRequestWhenConflicShouldLogKeyDetectedAndConflict()
        {
            var idempotencyKey = Guid.NewGuid().ToString();
            var idempotencyRegister = new IdempotencyRegister(idempotencyKey);
            var contextMock = new Mock<HttpContext>();
            var responseMock = new Mock<HttpResponse>();
            contextMock.SetupGet(x => x.Response)
                .Returns(responseMock.Object)
                .Verifiable();
            contextMock.Setup(x => x.Request.Headers.ContainsKey(IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY))
                .Returns(true)
                .Verifiable();
            contextMock.SetupGet(x => x.Request.Headers[IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY])
                .Returns(idempotencyKey)
                .Verifiable();
            responseMock.SetupSet(x => x.StatusCode = (int) HttpStatusCode.Conflict)
                .Verifiable();
            responseMock.SetupSet(x => x.ContentType = MediaTypeNames.Application.Json)
                .Verifiable();
            _repositoryMock.Setup(x => x.TryAddAsync(idempotencyKey))
                .ReturnsAsync(false)
                .Verifiable();
            _repositoryMock.Setup(x => x.GetAsync(idempotencyKey))
                .ReturnsAsync(idempotencyRegister)
                .Verifiable();
            _loggerMock.Setup(x => x.WriteInformation(idempotencyKey, "Idempotency: Key detected."))
                .Verifiable();
            _loggerMock.Setup(x =>
                    x.WriteResponse(idempotencyKey, "Idempotency: Conflict detected.", responseMock.Object))
                .Verifiable();

            await _middleware.InvokeAsync(contextMock.Object);

            contextMock.VerifyAll();
            responseMock.VerifyAll();
            _repositoryMock.VerifyAll();
            _nextMock.VerifyAll();
            _loggerMock.VerifyAll();
        }

        [Trait("Category", "Logging")]
        [Fact(DisplayName =
            "GIVEN Request, WHEN execution failed, SHOULD log key detected, AND first request, AND first request failed")]
        public async Task GivenRequestWhenExecutionFailedShouldLogKeyDetectedAndFirstRequestAndRequestFailed()
        {
            var idempotencyKey = Guid.NewGuid().ToString();
            var contextMock = new Mock<HttpContext>();
            var requestMock = new Mock<HttpRequest>();
            contextMock.Setup(x => x.Request)
                .Returns(requestMock.Object)
                .Verifiable();
            contextMock.SetupGet(x => x.Response.StatusCode)
                .Returns((int) HttpStatusCode.BadRequest)
                .Verifiable();
            requestMock.Setup(x => x.Headers.ContainsKey(IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY))
                .Returns(true)
                .Verifiable();
            requestMock.SetupGet(x => x.Headers[IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY])
                .Returns(idempotencyKey)
                .Verifiable();
            _repositoryMock.Setup(x => x.TryAddAsync(idempotencyKey))
                .ReturnsAsync(true)
                .Verifiable();
            _nextMock.Setup(x => x.Invoke(contextMock.Object))
                .Returns(Task.CompletedTask)
                .Verifiable();
            _loggerMock.Setup(x => x.WriteInformation(idempotencyKey, "Idempotency: Key detected."))
                .Verifiable();
            _loggerMock.Setup(x => x.WriteRequest(idempotencyKey, "Idempotency: First request.", requestMock.Object))
                .Verifiable();
            _loggerMock.Setup(x => x.WriteInformation(idempotencyKey, "Idempotency: First request failed."))
                .Verifiable();

            await _middleware.InvokeAsync(contextMock.Object);

            contextMock.VerifyAll();
            requestMock.VerifyAll();
            _repositoryMock.VerifyAll();
            _nextMock.VerifyAll();
            _loggerMock.VerifyAll();
        }

        [Trait("Category", "Logging")]
        [Fact(DisplayName =
            "GIVEN Request, WHEN execution successful, SHOULD log key detected, AND first request, AND first request completed")]
        public async Task GivenRequestWhenExecutionSuccessfulShouldLogKeyDetectedAndFirstRequestAndRequestCompleted()
        {
            var idempotencyKey = Guid.NewGuid().ToString();
            var contextMock = new Mock<HttpContext>();
            var requestMock = new Mock<HttpRequest>();
            var body = new MemoryStream(Encoding.UTF8.GetBytes("Successful Message"));
            contextMock.Setup(x => x.Request)
                .Returns(requestMock.Object)
                .Verifiable();
            contextMock.SetupGet(x => x.Response.StatusCode)
                .Returns((int) HttpStatusCode.OK)
                .Verifiable();
            contextMock.SetupGet(x => x.Response.ContentType)
                .Returns(MediaTypeNames.Application.Json)
                .Verifiable();
            contextMock.SetupGet(x => x.Response.Body)
                .Returns(body)
                .Verifiable();
            requestMock.Setup(x => x.Headers.ContainsKey(IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY))
                .Returns(true)
                .Verifiable();
            requestMock.SetupGet(x => x.Headers[IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY])
                .Returns(idempotencyKey)
                .Verifiable();
            _repositoryMock.Setup(x => x.TryAddAsync(idempotencyKey))
                .ReturnsAsync(true)
                .Verifiable();
            _repositoryMock.Setup(x => x.UpdateAsync(idempotencyKey, body))
                .Returns(Task.CompletedTask)
                .Verifiable();
            _nextMock.Setup(x => x.Invoke(contextMock.Object))
                .Returns(Task.CompletedTask)
                .Verifiable();
            _loggerMock.Setup(x => x.WriteInformation(idempotencyKey, "Idempotency: Key detected."))
                .Verifiable();
            _loggerMock.Setup(x => x.WriteRequest(idempotencyKey, "Idempotency: First request.", requestMock.Object))
                .Verifiable();
            _loggerMock.Setup(x => x.WriteInformation(idempotencyKey, "Idempotency: First request completed."))
                .Verifiable();

            await _middleware.InvokeAsync(contextMock.Object);

            contextMock.VerifyAll();
            requestMock.VerifyAll();
            _repositoryMock.VerifyAll();
            _nextMock.VerifyAll();
            _loggerMock.VerifyAll();
        }

        [Trait("Category", "Logging")]
        [Fact(DisplayName = "GIVEN Request, WHEN has response, SHOULD log key detected, AND Response from cache")]
        public async Task GivenRequestWhenHasResponseShouldLogKeyDetectedAndResponseFromCache()
        {
            var idempotencyKey = Guid.NewGuid().ToString();
            var idempotencyRegister = new IdempotencyRegister(
                idempotencyKey,
                HttpStatusCode.OK,
                new MemoryStream(Encoding.UTF8.GetBytes("Message cached.")));
            var contextMock = new Mock<HttpContext>();
            var responseMock = new Mock<HttpResponse>();
            contextMock.SetupGet(x => x.Response)
                .Returns(responseMock.Object)
                .Verifiable();
            contextMock.Setup(x => x.Request.Headers.ContainsKey(IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY))
                .Returns(true)
                .Verifiable();
            contextMock.SetupGet(x => x.Request.Headers[IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY])
                .Returns(idempotencyKey)
                .Verifiable();
            responseMock.SetupSet(x => x.StatusCode = (int) HttpStatusCode.OK)
                .Verifiable();
            responseMock.SetupSet(x => x.ContentType = MediaTypeNames.Application.Json)
                .Verifiable();
            responseMock.SetupSet(x => x.Body = It.IsAny<Stream>())
                .Verifiable();
            _repositoryMock.Setup(x => x.TryAddAsync(idempotencyKey))
                .ReturnsAsync(false)
                .Verifiable();
            _repositoryMock.Setup(x => x.GetAsync(idempotencyKey))
                .ReturnsAsync(idempotencyRegister)
                .Verifiable();
            _loggerMock.Setup(x => x.WriteInformation(idempotencyKey, "Idempotency: Key detected."))
                .Verifiable();
            _loggerMock.Setup(x =>
                    x.WriteResponse(idempotencyKey, "Idempotency: Response from cache.", responseMock.Object))
                .Verifiable();

            await _middleware.InvokeAsync(contextMock.Object);

            contextMock.VerifyAll();
            responseMock.VerifyAll();
            _repositoryMock.VerifyAll();
            _nextMock.VerifyAll();
            _loggerMock.VerifyAll();
        }

        [Trait("Category", "Cases")]
        [Fact(DisplayName =
            @"GIVEN Request, WHEN Idempotency-Key is avaiable AND an exception happens, SHOULD remove key AND throws")]
        public async Task GivenRequestWhenIdempotencyKeyAvaiableAndExceptionHappensShouldRemoveKeyAndThrow()
        {
            var idempotencyKey = Guid.NewGuid().ToString();
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(x => x.Request.Headers.ContainsKey(IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY))
                .Returns(true)
                .Verifiable();
            contextMock.SetupGet(x => x.Request.Headers[IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY])
                .Returns(idempotencyKey)
                .Verifiable();
            _repositoryMock.Setup(x => x.TryAddAsync(idempotencyKey))
                .ReturnsAsync(true)
                .Verifiable();
            _repositoryMock.Setup(x => x.RemoveAsync(idempotencyKey))
                .Returns(Task.CompletedTask)
                .Verifiable();
            _nextMock.Setup(x => x.Invoke(contextMock.Object))
                .ThrowsAsync(new InvalidOperationException())
                .Verifiable();

            Func<Task> action = () => _middleware.InvokeAsync(contextMock.Object);
            await action.Should().ThrowAsync<InvalidOperationException>();

            contextMock.VerifyAll();
            _repositoryMock.VerifyAll();
            _nextMock.VerifyAll();
        }

        [Trait("Category", "Cases")]
        [Fact(DisplayName =
            @"GIVEN Request, WHEN Idempotency-Key is avaiable AND fails to save key AND doesn't have response, SHOULD return StatusCode 409 - Conflict")]
        public async Task
            GivenRequestWhenIdempotencyKeyAvaiableAndFaildSaveKeyAndDoesntHaveResponseShouldReturnStatusCode409()
        {
            var idempotencyKey = Guid.NewGuid().ToString();
            var idempotencyRegister = new IdempotencyRegister(idempotencyKey);
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(x => x.Request.Headers.ContainsKey(IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY))
                .Returns(true)
                .Verifiable();
            contextMock.SetupGet(x => x.Request.Headers[IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY])
                .Returns(idempotencyKey)
                .Verifiable();
            contextMock.SetupSet(x => x.Response.StatusCode = (int) HttpStatusCode.Conflict)
                .Verifiable();
            contextMock.SetupSet(x => x.Response.ContentType = MediaTypeNames.Application.Json)
                .Verifiable();
            _repositoryMock.Setup(x => x.TryAddAsync(idempotencyKey))
                .ReturnsAsync(false)
                .Verifiable();
            _repositoryMock.Setup(x => x.GetAsync(idempotencyKey))
                .ReturnsAsync(idempotencyRegister)
                .Verifiable();

            await _middleware.InvokeAsync(contextMock.Object);

            contextMock.VerifyAll();
            _repositoryMock.VerifyAll();
            _nextMock.VerifyAll();
        }

        [Trait("Category", "Cases")]
        [Fact(DisplayName =
            @"GIVEN Request, WHEN Idempotency-Key is avaiable AND fails to save key AND has response, SHOULD return response")]
        public async Task GivenRequestWhenIdempotencyKeyAvaiableAndFailsSaveKeyAndHasResponseShouldReturnResponse()
        {
            var idempotencyKey = Guid.NewGuid().ToString();
            var idempotencyRegister = new IdempotencyRegister(
                idempotencyKey,
                HttpStatusCode.OK,
                new MemoryStream(Encoding.UTF8.GetBytes("Successful saved message")));
            string responseBody = null;
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(x => x.Request.Headers.ContainsKey(IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY))
                .Returns(true)
                .Verifiable();
            contextMock.SetupGet(x => x.Request.Headers[IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY])
                .Returns(idempotencyKey)
                .Verifiable();
            contextMock.SetupSet(x => x.Response.StatusCode = (int) HttpStatusCode.OK)
                .Verifiable();
            contextMock.SetupSet(x => x.Response.ContentType = MediaTypeNames.Application.Json)
                .Verifiable();
            contextMock.SetupSet(x => x.Response.Body = It.IsAny<Stream>())
                .Callback((Stream stream) =>
                {
                    using (var sr = new StreamReader(stream))
                    {
                        responseBody = sr.ReadToEnd();
                    }

                    ;
                })
                .Verifiable();
            _repositoryMock.Setup(x => x.TryAddAsync(idempotencyKey))
                .ReturnsAsync(false)
                .Verifiable();
            _repositoryMock.Setup(x => x.GetAsync(idempotencyKey))
                .ReturnsAsync(idempotencyRegister)
                .Verifiable();

            await _middleware.InvokeAsync(contextMock.Object);

            contextMock.VerifyAll();
            _repositoryMock.VerifyAll();
            _nextMock.VerifyAll();
            responseBody.Should()
                .NotBeNullOrWhiteSpace()
                .And.Be("Successful saved message");
        }

        [Trait("Category", "Cases")]
        [Fact(DisplayName =
            @"GIVEN Request, WHEN Idempotency-Key is avaiable AND execution is successful, SHOULD save key and response from application")]
        public async Task
            GivenRequestWhenIdempotencyKeyIsAvaiableAndExecutionSuccessfulShouldSaveKeyAndResponseFromApplication()
        {
            var idempotencyKey = Guid.NewGuid().ToString();
            var contextMock = new Mock<HttpContext>();
            var body = new MemoryStream(Encoding.UTF8.GetBytes("Successful Message"));
            contextMock.Setup(x => x.Request.Headers.ContainsKey(IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY))
                .Returns(true)
                .Verifiable();
            contextMock.SetupGet(x => x.Request.Headers[IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY])
                .Returns(idempotencyKey)
                .Verifiable();
            contextMock.SetupGet(x => x.Response.StatusCode)
                .Returns((int) HttpStatusCode.OK)
                .Verifiable();
            contextMock.SetupGet(x => x.Response.ContentType)
                .Returns(MediaTypeNames.Application.Json)
                .Verifiable();
            contextMock.SetupGet(x => x.Response.Body)
                .Returns(body)
                .Verifiable();
            _repositoryMock.Setup(x => x.TryAddAsync(idempotencyKey))
                .ReturnsAsync(true)
                .Verifiable();
            _repositoryMock.Setup(x => x.UpdateAsync(idempotencyKey, body))
                .Returns(Task.CompletedTask)
                .Verifiable();
            _nextMock.Setup(x => x.Invoke(contextMock.Object))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await _middleware.InvokeAsync(contextMock.Object);

            contextMock.VerifyAll();
            _repositoryMock.VerifyAll();
            _nextMock.VerifyAll();
        }

        [Trait("Category", "Cases")]
        [Fact(DisplayName = @"GIVEN Request, WHEN Idempotency-Key isn't avaiable, SHOULD call application")]
        public async Task GivenRequestWhenIdempotencyKeyNotAvaiableShouldCallApplication()
        {
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(x => x.Request.Headers.ContainsKey(IdempotencyMiddleware.IDEMPOTENCY_HEADER_KEY))
                .Returns(false)
                .Verifiable();
            _nextMock.Setup(x => x.Invoke(contextMock.Object))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await _middleware.InvokeAsync(contextMock.Object);

            contextMock.VerifyAll();
            _nextMock.VerifyAll();
        }
    }
}