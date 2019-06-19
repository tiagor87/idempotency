using System;
using System.IO;
using System.Net;

namespace Idempotency.Core
{
    public sealed class IdempotencyRegister
    {
        public IdempotencyRegister(string key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public IdempotencyRegister(string key, HttpStatusCode statusCode, Stream body) : this(key)
        {
            Response = new IdempotencyResponse(statusCode, body);
        }

        public string Key { get; }
        public IdempotencyResponse Response { get; }
    }
}