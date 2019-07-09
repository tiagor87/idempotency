using System;
using ZeroFormatter;

namespace Idempotency.Core
{
    [ZeroFormattable]
    public class IdempotencyRegister : IIdempotencyRegister
    {
        public IdempotencyRegister()
        {
        }

        private IdempotencyRegister(string key, string value) : this()
        {
            Key = key;
            IsCompleted = true;
            Value = value;
        }

        private IdempotencyRegister(string key) : this()
        {
            Key = key;
            IsCompleted = false;
            Value = null;
        }

        [Index(0)] public virtual string Key { get; protected set; }

        [Index(1)] public virtual bool IsCompleted { get; protected set; }

        [Index(2)] public virtual string Value { get; protected set; }

        public static IdempotencyRegister Of(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return new IdempotencyRegister(key, null);
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