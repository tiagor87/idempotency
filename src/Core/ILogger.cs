using System;
using Microsoft.AspNetCore.Http;

namespace Idempotency.Core
{
    public interface ILogger
    {
        void WriteRequest(string key, string message, HttpRequest request);
        void WriteInformation(string key, string message);
        void WriteException(string key, Exception ex);
        void WriteResponse(string key, string message, HttpResponse response);
    }
}