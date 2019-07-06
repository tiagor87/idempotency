using System;
using System.IO;
using ZeroFormatter;

namespace Idempotency.Core
{
    [ZeroFormattable]
    public class IdempotencyRegister
    {
        public IdempotencyRegister()
        {
        }

        private IdempotencyRegister(string key, string body) : this()
        {
            Key = key;
            Completed = true;
            Body = body;
        }

        private IdempotencyRegister(string key) : this()
        {
            Key = key;
            Completed = null;
            Body = null;
        }

        [Index(0)] public virtual string Key { get; protected set; }

        [Index(1)] public virtual bool? Completed { get; protected set; }

        [Index(2)] public virtual string Body { get; protected set; }

        public static IdempotencyRegister Of(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return new IdempotencyRegister(key, null);
        }

        public static IdempotencyRegister Of(string key, Stream stream)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (var reader = new StreamReader(stream))
            {
                return new IdempotencyRegister(key, reader.ReadToEnd());
            }
        }

        public static IdempotencyRegister Of(string key, string body)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return new IdempotencyRegister(key, body);
        }
    }
}