using System;

namespace Idempotency.Core
{
    public class ConflictDetectedException : Exception
    {
        public ConflictDetectedException(string idempotencyKey) : base($@"Conflict detected for key ""{idempotencyKey}"".")
        {
        }
    }
}