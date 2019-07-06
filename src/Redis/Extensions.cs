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
        public static void AddRedisIdempotency(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IIdempotencyKeyReader<HttpRequest>, HttpRequestIdempotencyKeyReader>();
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));
            services.AddScoped(provider =>
            {
                var connection = provider.GetService<IConnectionMultiplexer>();
                return connection.GetDatabase();
            });
            services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
        }

        public static void UseIdempotency(this IApplicationBuilder app)
        {
            app.UseMiddleware<IdempotencyMiddleware>();
        }
    }
}