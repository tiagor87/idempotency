using System;
using System.IO;
using System.Net;
using ZeroFormatter;

namespace Idempotency.Core
{
    [ZeroFormattable]
    public class IdempotencyRegister
    {
        public IdempotencyRegister()
        {
        }

        private IdempotencyRegister(string key, int? statusCode, string body) : this()
        {
            Key = key;
            StatusCode = statusCode;
            Body = body;
        }

        [Index(0)] public virtual string Key { get; protected set; }

        [Index(1)] public virtual int? StatusCode { get; protected set; }

        [Index(2)] public virtual string Body { get; protected set; }

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