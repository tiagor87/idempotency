using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Idempotency.Core
{
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public IdempotencyMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var logger = scope.ServiceProvider.GetService<ILogger>();
                var repository = scope.ServiceProvider.GetService<IIdempotencyRepository>();
                var keyReader = scope.ServiceProvider.GetService<IIdempotencyKeyReader>();

                var idempotencyKey = keyReader.Read(context.Request);

                if (string.IsNullOrWhiteSpace(idempotencyKey) ||
                    HttpMethods.IsGet(context.Request.Method))
                {
                    await _next.Invoke(context);
                    return;
                }

                logger?.WriteInformation(idempotencyKey, "Idempotency: Key detected.");
                if (await repository.TryAddAsync(idempotencyKey))
                {
                    using (var stream = new MemoryStream())
                    {
                        logger?.WriteRequest(idempotencyKey, "Idempotency: First request.", context.Request);
                        try
                        {
                            var originalResponseBody = context.Response.Body;
                            context.Response.Body = stream;
                            await _next.Invoke(context);
                            stream.Seek(0, SeekOrigin.Begin);
                            await stream.CopyToAsync(originalResponseBody);
                            stream.Seek(0, SeekOrigin.Begin);
                        }
                        catch (Exception ex)
                        {
                            await repository.RemoveAsync(idempotencyKey);
                            logger?.WriteException(idempotencyKey, ex);
                            throw;
                        }

                        if (context.Response.StatusCode >= (int) HttpStatusCode.BadRequest
                            || context.Response.ContentType != null
                            && !context.Response.ContentType.Contains("application/json"))
                        {
                            await repository.RemoveAsync(idempotencyKey);
                            logger?.WriteInformation(idempotencyKey, "Idempotency: First request failed.");
                            return;
                        }

                        var updatedRegister = IdempotencyRegister.Of(idempotencyKey,
                            (HttpStatusCode) context.Response.StatusCode,
                            stream);
                        await repository.UpdateAsync(idempotencyKey, updatedRegister);
                        logger?.WriteInformation(idempotencyKey, "Idempotency: First request completed.");
                        return;
                    }
                }

                var register = await repository.GetAsync(idempotencyKey);
                if (register.StatusCode == null)
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