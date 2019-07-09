using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Idempotency.Core
{
    public class HttpRequestIdempotencyKeyReader : IIdempotencyKeyReader<HttpRequest>
    {
        public const string IDEMPOTENCY_KEY = "Idempotency-Key";

        public string Read(HttpRequest request)
        {
            var headerKey = request.Headers.Keys.FirstOrDefault(key =>
                key.Equals(IDEMPOTENCY_KEY, StringComparison.InvariantCultureIgnoreCase));
            if (headerKey == null
                || string.IsNullOrWhiteSpace(request.Headers[headerKey]))
            {
                return null;
            }

            return $"[{request.Method}] {request.Path} - {request.Headers[headerKey]}";
        }
    }
}