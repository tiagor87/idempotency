using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Idempotency.Core;
using Idempotency.Redis.UnitTests.Stub;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Idempotency.Redis.UnitTests
{
    public class ExtensionsTests : IClassFixture<TestApplicationService>
    {
        public ExtensionsTests(TestApplicationService factory)
        {
            _factory = factory;
        }

        private readonly TestApplicationService _factory;

        [Trait("Category", "Redis - Extensions")]
        [Fact(DisplayName = "GIVEN Application Builder, WHEN use Idempotency, SHOULD register middleware")]
        public async Task GivenApplicationBuilderWhenUseIdempotencyShouldRegisterMiddleware()
        {
            var key = Guid.NewGuid().ToString();
            var loggerMock = new Mock<ILogger<HttpRequest, HttpResponse>>();
            loggerMock.Setup(x =>
                    x.WriteRequest(It.Is<string>(y => y.Contains(key)), It.IsAny<string>(), It.IsAny<HttpRequest>()))
                .Verifiable();
            var client = _factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services => services.AddScoped(_ => loggerMock.Object));
                })
                .CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Post, "/test");
            request.Headers.Add(HttpRequestIdempotencyKeyReader.IDEMPOTENCY_KEY, key);
            request.Content = new StringContent("{}", Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            loggerMock.VerifyAll();
        }

        [Trait("Category", "Redis - Extensions")]
        [Fact(DisplayName = "GIVEN Service Collection, WHEN Add Redis Idempotency, SHOULD Register services")]
        public void GivenServiceCollectionWhenAddRedisIdempotencyShouldRegisterServices()
        {
            _factory.CreateClient();

            var keyReader = _factory.Server.Host.Services.GetService<IIdempotencyKeyReader<HttpRequest>>();
            var repository = _factory.Server.Host.Services.GetService<IIdempotencyRepository>();
            var serializer = _factory.Server.Host.Services.GetService<IIdempotencySerializer>();

            keyReader.Should().NotBeNull()
                .And.BeOfType<HttpRequestIdempotencyKeyReader>();
            repository.Should().NotBeNull()
                .And.BeOfType<IdempotencyRepository>();
            serializer.Should().NotBeNull()
                .And.BeOfType<IdempotencySerializer>();
        }
    }
}