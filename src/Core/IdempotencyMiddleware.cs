using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Idempotency.Core
{
    public class IdempotencyMiddleware
    {
        public const string IDEMPOTENCY_HEADER_KEY = "Idempotency-Key";
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;
        private readonly IIdempotencyRepository _repository;

        public IdempotencyMiddleware(RequestDelegate next, IIdempotencyRepository repository, ILogger logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey(IDEMPOTENCY_HEADER_KEY))
            {
                await _next.Invoke(context);
                return;
            }

            var idempotencyKey = context.Request.Headers[IDEMPOTENCY_HEADER_KEY];
            _logger.WriteInformation(idempotencyKey, "Idempotency: Key detected.");
            if (await _repository.TryAddAsync(idempotencyKey))
            {
                _logger.WriteRequest(idempotencyKey, "Idempotency: First request.", context.Request);
                try
                {
                    await _next.Invoke(context);
                }
                catch (Exception ex)
                {
                    await _repository.RemoveAsync(idempotencyKey);
                    _logger.WriteException(idempotencyKey, ex);
                    throw;
                }

                if (context.Response.StatusCode >= (int) HttpStatusCode.BadRequest
                    || context.Response.ContentType != "application/json")
                {
                    await _repository.RemoveAsync(idempotencyKey);
                    _logger.WriteInformation(idempotencyKey, "Idempotency: First request failed.");
                    return;
                }

                await _repository.UpdateAsync(idempotencyKey, context.Response.Body);
                _logger.WriteInformation(idempotencyKey, "Idempotency: First request completed.");
                return;
            }

            var register = await _repository.GetAsync(idempotencyKey);
            if (register.Response == null)
            {
                context.Response.StatusCode = (int) HttpStatusCode.Conflict;
                context.Response.ContentType = "application/json";
                _logger.WriteResponse(idempotencyKey, "Idempotency: Conflict detected.", context.Response);
                return;
            }

            context.Response.StatusCode = register.Response.StatusCode;
            context.Response.ContentType = "application/json";
            context.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(register.Response.Body));
            _logger.WriteResponse(idempotencyKey, "Idempotency: Response from cache.", context.Response);
        }
    }
}