using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace Idempotency.Redis.UnitTests.Stub
{
    public class TestApplicationService : WebApplicationFactory<TestStartup>
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return new WebHostBuilder()
                .UseStartup<TestStartup>();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSolutionRelativeContentRoot("./tests/Redis/UnitTests");
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("CONNECTIONSTRINGS:REDIS", "localhost:6379"),
                });
            });
        }
    }
}