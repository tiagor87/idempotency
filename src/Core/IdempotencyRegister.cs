using System;
using System.IO;

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

        public static IdempotencyRegister Of(string key, int statusCode, Stream stream)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (statusCode < 0 || statusCode >= 600)
            {
                throw new ArgumentOutOfRangeException(nameof(key));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (var reader = new StreamReader(stream))
            {
                return new IdempotencyRegister(key, statusCode, reader.ReadToEnd());
            }
        }
    }
}