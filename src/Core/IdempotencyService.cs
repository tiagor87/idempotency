using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ResultCore;

namespace Idempotency.Core
{
    public class IdempotencyService<TRequest, TResponse>
        where TResponse : IResult
    {
        private readonly IIdempotencyKeyReader<TRequest> _keyReader;
        private readonly ILogger<TRequest, TResponse> _logger;
        private readonly Func<Task<TResponse>> _next;
        private readonly IIdempotencyRepository _repository;

        public IdempotencyService(ILogger<TRequest, TResponse> logger, IIdempotencyRepository repository,
            IIdempotencyKeyReader<TRequest> keyReader, Func<Task<TResponse>> next)
        {
            _logger = logger;
            _repository = repository;
            _keyReader = keyReader;
            _next = next;
        }

        public async Task<IResult> Execute(TRequest request)
        {
            var idempotencyKey = _keyReader.Read(request);

            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                return await _next();
            }

            _logger?.WriteInformation(idempotencyKey, "Idempotency: Key detected.");
            if (await _repository.TryAddAsync(idempotencyKey))
            {
                TResponse response = default;
                _logger?.WriteRequest(idempotencyKey, "Idempotency: First request.", request);
                try
                {
                    response = await _next.Invoke();
                }
                catch (Exception ex)
                {
                    await _repository.RemoveAsync(idempotencyKey);
                    _logger?.WriteException(idempotencyKey, ex);
                    throw;
                }

                if (response.Failure)
                {
                    await _repository.RemoveAsync(idempotencyKey);
                    _logger?.WriteInformation(idempotencyKey, "Idempotency: First request failed.");
                    return Result.Fail(response.Message);
                }

                var updatedRegister = IdempotencyRegister.Of(idempotencyKey, response.As<string>());
                await _repository.UpdateAsync(idempotencyKey, updatedRegister);
                _logger?.WriteInformation(idempotencyKey, "Idempotency: First request completed.");
                return response;
            }

            var register = await _repository.GetAsync(idempotencyKey);
            if (register.Completed == false)
            {
                context.Response.StatusCode = (int) HttpStatusCode.Conflict;
                context.Response.ContentType = "application/json";
                logger?.WriteResponse(idempotencyKey, "Idempotency: Conflict detected.", context.Response);
                return;
            }

            context.Response.StatusCode = register.StatusCode.Value;
            context.Response.ContentType = "application/json";
            using (var body = new MemoryStream(Encoding.UTF8.GetBytes(register.Body)))
            {
                await body.CopyToAsync(context.Response.Body);
            }

            logger?.WriteResponse(idempotencyKey, "Idempotency: Response from cache.", context.Response);
        }
    }
}

}