using System;
using Idempotency.Core;
using Idempotency.Samples.Redis.Core.Commands;

namespace Idempotency.Samples.Redis.Core.Logger
{
    public class IdempotencyLogger : ILogger<IncrementCounter, int>
    {
        
        public void WriteRequest(string key, string message, IncrementCounter request)
        {            
        }

        public void WriteInformation(string key, string message)
        {
        }

        public void WriteException(string key, Exception ex)
        {
        }

        public void WriteResponse(string key, string message, int response)
        {
        }
    }
}