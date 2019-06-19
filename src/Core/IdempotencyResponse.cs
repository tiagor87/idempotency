using System.IO;
using System.Net;

namespace Idempotency.Core
{
    public sealed class IdempotencyResponse
    {
        public IdempotencyResponse(HttpStatusCode statusCode, Stream body)
        {
            StatusCode = (int) statusCode;
            using (var streamReader = new StreamReader(body))
            {
                Body = streamReader.ReadToEnd();
            }
        }

        public int StatusCode { get; }
        public string Body { get; }
    }
}