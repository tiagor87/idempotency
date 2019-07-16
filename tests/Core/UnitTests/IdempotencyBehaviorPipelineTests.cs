using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using ResultCore;
using Xunit;

namespace Idempotency.Core.UnitTests
{
    public class IdempotencyBehaviorPipelineTests
    {
        public IdempotencyBehaviorPipelineTests()
        {
            _requestMock = new Mock<IRequest<IResult>>();
            _nextMock = new Mock<RequestHandlerDelegate<IResult>>();
            _keyReaderMock = new Mock<IIdempotencyKeyReader<IRequest<IResult>>>();
            _repositoryMock = new Mock<IIdempotencyRepository>();
            _serializerMock = new Mock<IIdempotencySerializer>();
            _loggerMock = new Mock<ILogger<IRequest<IResult>, IResult>>();
            _pipeline = new IdempotencyPipelineBehavior<IRequest<IResult>, IResult>(
                _keyReaderMock.Object,
                _repositoryMock.Object,
                _serializerMock.Object,
                _loggerMock.Object);
        }

        private readonly IdempotencyPipelineBehavior<IRequest<IResult>, IResult> _pipeline;
        private readonly Mock<IRequest<IResult>> _requestMock;
        private readonly Mock<RequestHandlerDelegate<IResult>> _nextMock;
        private readonly Mock<IIdempotencyKeyReader<IRequest<IResult>>> _keyReaderMock;
        private readonly Mock<IIdempotencyRepository> _repositoryMock;
        private readonly Mock<IIdempotencySerializer> _serializerMock;
        private readonly Mock<ILogger<IRequest<IResult>, IResult>> _loggerMock;

        [Trait("Category", "Cases")]
        [Theory(DisplayName = @"GIVEN Request, WHEN Idempotency-Key isn't avaiable, SHOULD call application")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GivenRequestWhenIdempotencyKeyNotAvaiableShouldCallApplication(string key)
        {
            _keyReaderMock.Setup(x => x.Read(It.IsAny<IRequest<IResult>>()))
                .Returns(key)
                .Verifiable();
            _nextMock.Setup(x => x.Invoke())
                .ReturnsAsync(Result.Success())
                .Verifiable();

            var result = await _pipeline.Handle(
                _requestMock.Object,
                CancellationToken.None,
                _nextMock.Object);

            result.Should().NotBeNull();
            result.Successful.Should().BeTrue();

            _repositoryMock.Verify(x => x.TryAddAsync(It.IsAny<string>()), Times.Never());
            _nextMock.VerifyAll();
        }

        [Trait("Category", "Cases")]
        [Theory(DisplayName = @"GIVEN Request, WHEN KeyReader isn't avaiable, SHOULD call application")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GivenRequestWhenKeyReaderNotAvaiableShouldCallApplication(string key)
        {
            var pipeline = new IdempotencyPipelineBehavior<IRequest<IResult>, IResult>(
                null,
                _repositoryMock.Object,
                _serializerMock.Object,
                _loggerMock.Object);

            _nextMock.Setup(x => x.Invoke())
                .ReturnsAsync(Result.Success())
                .Verifiable();

            var result = await pipeline.Handle(
                _requestMock.Object,
                CancellationToken.None,
                _nextMock.Object);

            result.Should().NotBeNull();
            result.Successful.Should().BeTrue();

            _repositoryMock.Verify(x => x.TryAddAsync(It.IsAny<string>()), Times.Never());
            _nextMock.VerifyAll();
        }

        [Trait("Category", "Logging")]
        [Fact(DisplayName = "GIVEN Request, WHEN conflict, SHOULD log key detected, AND conflict")]
        public async Task GivenRequestWhenConflicShouldLogKeyDetectedAndConflict()
        {
        }

        [Trait("Category", "Logging")]
        [Fact(DisplayName =
            "GIVEN Request, WHEN execution failed, SHOULD log key detected, AND first request, AND first request failed")]
        public async Task GivenRequestWhenExecutionFailedShouldLogKeyDetectedAndFirstRequestAndRequestFailed()
        {
        }

        [Trait("Category", "Cases")]
        [Fact(DisplayName =
            @"GIVEN Request, WHEN execution fails, SHOULD remove key")]
        public async Task GivenRequestWhenExecutionFailsShouldRemoveKey()
        {
            var key = Guid.NewGuid().ToString();
            _keyReaderMock.Setup(x => x.Read(It.IsAny<IRequest<IResult>>()))
                .Returns(key)
                .Verifiable();
            _nextMock.Setup(x => x.Invoke())
                .ReturnsAsync(Result.Fail("Execution failed message."))
                .Verifiable();
            _repositoryMock.Setup(x => x.TryAddAsync(key))
                .ReturnsAsync(true)
                .Verifiable();
            _repositoryMock.Setup(x => x.RemoveAsync(key))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var result = await _pipeline.Handle(
                _requestMock.Object,
                CancellationToken.None,
                _nextMock.Object);

            result.Should().NotBeNull();
            result.Failure.Should().BeTrue();

            _keyReaderMock.VerifyAll();
            _nextMock.VerifyAll();
            _repositoryMock.VerifyAll();
        }

        [Trait("Category", "Logging")]
        [Fact(DisplayName =
            "GIVEN Request, WHEN execution successful, SHOULD log key detected, AND first request, AND first request completed")]
        public async Task GivenRequestWhenExecutionSuccessfulShouldLogKeyDetectedAndFirstRequestAndRequestCompleted()
        {
        }

        [Trait("Category", "Logging")]
        [Fact(DisplayName = "GIVEN Request, WHEN has response, SHOULD log key detected, AND Response from cache")]
        public async Task GivenRequestWhenHasResponseShouldLogKeyDetectedAndResponseFromCache()
        {
        }

        [Trait("Category", "Cases")]
        [Fact(DisplayName =
            @"GIVEN Request, WHEN Idempotency-Key is avaiable AND an exception happens, SHOULD remove key AND throws")]
        public async Task GivenRequestWhenIdempotencyKeyAvaiableAndExceptionHappensShouldRemoveKeyAndThrow()
        {
            var key = Guid.NewGuid().ToString();
            _keyReaderMock.Setup(x => x.Read(It.IsAny<IRequest<IResult>>()))
                .Returns(key)
                .Verifiable();
            _repositoryMock.Setup(x => x.TryAddAsync(key))
                .ReturnsAsync(true)
                .Verifiable();
            _nextMock.Setup(x => x.Invoke())
                .Throws<InvalidOperationException>()
                .Verifiable();
            _repositoryMock.Setup(x => x.RemoveAsync(key))
                .Returns(Task.CompletedTask)
                .Verifiable();

            Func<Task<IResult>> action = () => _pipeline.Handle(
                _requestMock.Object,
                CancellationToken.None,
                _nextMock.Object);

            await action.Should().ThrowAsync<InvalidOperationException>();

            _keyReaderMock.VerifyAll();
            _repositoryMock.VerifyAll();
            _nextMock.VerifyAll();
        }

        [Trait("Category", "Cases")]
        [Fact(DisplayName =
            @"GIVEN Request, WHEN Idempotency-Key is avaiable AND fails to save key AND doesn't have response, SHOULD return StatusCode 409 - Conflict")]
        public async Task
            GivenRequestWhenIdempotencyKeyAvaiableAndFailsSaveKeyAndDoesntHaveResponseShouldReturnStatusCode409()
        {
            var key = Guid.NewGuid().ToString();
            _keyReaderMock.Setup(x => x.Read(It.IsAny<IRequest<IResult>>()))
                .Returns(key)
                .Verifiable();
            _repositoryMock.Setup(x => x.TryAddAsync(key))
                .ReturnsAsync(false)
                .Verifiable();
            _repositoryMock.Setup(x => x.GetAsync<IdempotencyRegister>(key))
                .ReturnsAsync(IdempotencyRegister.Of(key))
                .Verifiable();

            Func<Task<IResult>> action = () => _pipeline.Handle(
                _requestMock.Object,
                CancellationToken.None,
                _nextMock.Object);

            await action.Should().ThrowAsync<ConflictDetectedException>();

            _keyReaderMock.VerifyAll();
            _repositoryMock.VerifyAll();
        }

        [Trait("Category", "Cases")]
        [Fact(DisplayName =
            @"GIVEN Request, WHEN Idempotency-Key is avaiable AND fails to save key AND has response, SHOULD return response")]
        public async Task GivenRequestWhenIdempotencyKeyAvaiableAndFailsSaveKeyAndHasResponseShouldReturnResponse()
        {
            var key = Guid.NewGuid().ToString();
            _keyReaderMock.Setup(x => x.Read(It.IsAny<IRequest<IResult>>()))
                .Returns(key)
                .Verifiable();
            _repositoryMock.Setup(x => x.TryAddAsync(key))
                .ReturnsAsync(false)
                .Verifiable();
            _repositoryMock.Setup(x => x.GetAsync<IdempotencyRegister>(key))
                .ReturnsAsync(IdempotencyRegister.Of(key, "body"))
                .Verifiable();
            _serializerMock.Setup(x => x.Deserialize<IResult>(It.IsAny<string>()))
                .Returns(Result<string>.Success("body"))
                .Verifiable();

            var result = await _pipeline.Handle(
                _requestMock.Object,
                CancellationToken.None,
                _nextMock.Object);

            result.Should().NotBeNull();
            result.Successful.Should().BeTrue();
            result.As<string>().Should().Be("body");
            _keyReaderMock.VerifyAll();
            _repositoryMock.VerifyAll();
            _serializerMock.VerifyAll();
        }

        [Trait("Category", "Cases")]
        [Fact(DisplayName =
            @"GIVEN Request, WHEN Idempotency-Key is avaiable AND execution is successful, SHOULD save key and response from application")]
        public async Task
            GivenRequestWhenIdempotencyKeyIsAvaiableAndExecutionSuccessfulShouldSaveKeyAndResponseFromApplication()
        {
            var key = Guid.NewGuid().ToString();
            _keyReaderMock.Setup(x => x.Read(It.IsAny<IRequest<IResult>>()))
                .Returns(key)
                .Verifiable();
            _repositoryMock.Setup(x => x.TryAddAsync(key))
                .ReturnsAsync(true)
                .Verifiable();
            _nextMock.Setup(x => x.Invoke())
                .ReturnsAsync(Result.Success("body"))
                .Verifiable();
            _repositoryMock.Setup(x => x.UpdateAsync(key, It.IsAny<IdempotencyRegister>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var result = await _pipeline.Handle(
                _requestMock.Object,
                CancellationToken.None,
                _nextMock.Object);

            result.Should().NotBeNull();
            result.Successful.Should().BeTrue();
            result.As<string>().Should().Be("body");

            _keyReaderMock.VerifyAll();
            _repositoryMock.VerifyAll();
            _nextMock.VerifyAll();
        }
    }
}