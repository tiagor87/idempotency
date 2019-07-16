using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ResultCore;

namespace Idempotency.Core
{
    public class IdempotencyPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IIdempotencySerializer _idempotencySerializer;
        private readonly IIdempotencyKeyReader<TRequest> _keyReader;
        private readonly ILogger<TRequest, TResponse> _logger;
        private readonly IIdempotencyRepository _repository;

        public IdempotencyPipelineBehavior(IIdempotencyKeyReader<TRequest> keyReader,
            IIdempotencyRepository repository,
            IIdempotencySerializer idempotencySerializer,
            ILogger<TRequest, TResponse> logger = null)
        {
            _keyReader = keyReader;
            _logger = logger;
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _idempotencySerializer =
                idempotencySerializer ?? throw new ArgumentNullException(nameof(idempotencySerializer));
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var idempotencyKey = _keyReader?.Read(request);
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                return await next();
            }

            TResponse response;
            _logger?.WriteInformation(idempotencyKey, "Idempotency: Key detected.");
            if (await _repository.TryAddAsync(idempotencyKey))
            {
                _logger?.WriteRequest(idempotencyKey, "Idempotency: First request.", request);
                try
                {
                    response = await next();
                }
                catch (Exception ex)
                {
                    await _repository.RemoveAsync(idempotencyKey);
                    _logger?.WriteException(idempotencyKey, ex);
                    throw;
                }

                if (response is IResult result && result.Failure)
                {
                    await _repository.RemoveAsync(idempotencyKey);
                    _logger?.WriteInformation(idempotencyKey, "Idempotency: First request failed.");
                    return response;
                }

                var updatedRegister =
                    IdempotencyRegister.Of(idempotencyKey, _idempotencySerializer.Serialize(response));
                await _repository.UpdateAsync(idempotencyKey, updatedRegister);
                _logger?.WriteInformation(idempotencyKey, "Idempotency: First request completed.");
                return response;
            }

            var register = await _repository.GetAsync<IdempotencyRegister>(idempotencyKey);
            if (!register.IsCompleted)
            {
                _logger?.WriteResponse(idempotencyKey, "Idempotency: Conflict detected.", default);
                throw new ConflictDetectedException(idempotencyKey);
            }

            response = _idempotencySerializer.Deserialize<TResponse>(register.Value);
            _logger?.WriteResponse(idempotencyKey, "Idempotency: Conflict detected.", response);

            return response;
        }
    }
}