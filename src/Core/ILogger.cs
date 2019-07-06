using System;

namespace Idempotency.Core
{
    public interface ILogger<TRequest, TResponse>
    {
        void WriteRequest(string key, string message, TRequest request);
        void WriteInformation(string key, string message);
        void WriteException(string key, Exception ex);
        void WriteResponse(string key, string message, TResponse response);
    }
}