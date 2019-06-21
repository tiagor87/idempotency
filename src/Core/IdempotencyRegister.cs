using System;
using System.IO;
using System.Net;

namespace Idempotency.Core
{
    public sealed class IdempotencyRegister
    {
        public IdempotencyRegister(string key, int? statusCode, string body)
        {
            Key = key;
            StatusCode = statusCode;
            Body = body;
        }

        public string Key { get; }
        public int? StatusCode { get; }
        public string Body { get; }

        public static IdempotencyRegister Of(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return new IdempotencyRegister(key, null, null);
        }

        public static IdempotencyRegister Of(string key, HttpStatusCode statusCode, Stream stream)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (statusCode >= HttpStatusCode.BadRequest)
            {
                throw new ArgumentOutOfRangeException(nameof(key));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (var reader = new StreamReader(stream))
            {
                return new IdempotencyRegister(key, (int) statusCode, reader.ReadToEnd());
            }
        }
    }
}