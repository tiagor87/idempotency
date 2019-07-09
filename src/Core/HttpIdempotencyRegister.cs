using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using ZeroFormatter;

namespace Idempotency.Core
{
    [ZeroFormattable]
    public class HttpIdempotencyRegister : IIdempotencyRegister
    {
        [ExcludeFromCodeCoverage]
        public HttpIdempotencyRegister()
        {
        }
        
        private HttpIdempotencyRegister(string key, HttpStatusCode statusCode, string value)
        {
            Key = key;
            IsCompleted = true;
            StatusCode = statusCode;
            Value = value;
        }

        private HttpIdempotencyRegister(string key)
        {
            Key = key;
            IsCompleted = false;
            Value = null;
        }

        [Index(0)] public virtual string Key { get; protected set; }

        [Index(1)] public virtual bool IsCompleted { get; protected set; }
        [Index(2)] public virtual string Value { get; protected set; }
        [Index(3)] public virtual HttpStatusCode StatusCode { get; protected set; }
        
        public static HttpIdempotencyRegister Of(string key, HttpStatusCode statusCode, Stream stream)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (statusCode >= HttpStatusCode.BadRequest)
            {
                throw new ArgumentException("The status code should be a success.", nameof(statusCode));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (var reader = new StreamReader(stream))
            {
                return new HttpIdempotencyRegister(key, statusCode, reader.ReadToEnd());
            }
        }
        
        public static HttpIdempotencyRegister Of(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return new HttpIdempotencyRegister(key);
        }
    }
}