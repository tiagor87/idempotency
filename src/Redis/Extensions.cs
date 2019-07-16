using Idempotency.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Idempotency.Redis
{
    public static class Extensions
    {
        public static IServiceCollection AddRedisIdempotency(this IServiceCollection services,
            IConfiguration configuration)
        {
            return services
                .AddRedis(configuration)
                .AddScoped<IIdempotencyKeyReader<HttpRequest>, HttpRequestIdempotencyKeyReader>()
                .AddScoped<IIdempotencyRepository, IdempotencyRepository>()
                .AddScoped<IIdempotencySerializer, IdempotencySerializer>();
        }

        public static IApplicationBuilder UseIdempotency(this IApplicationBuilder app)
        {
            return app.UseMiddleware<IdempotencyMiddleware>();
        }

        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddSingleton<IConnectionMultiplexer>(
                    ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")))
                .AddScoped(provider =>
                {
                    var connection = provider.GetService<IConnectionMultiplexer>();
                    return connection.GetDatabase();
                });
        }
    }
}