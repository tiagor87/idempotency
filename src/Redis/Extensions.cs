using Idempotency.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Redis;

namespace Idempotency.Redis
{
    public static class Extensions
    {
        public static void AddRedisIdempotency(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IIdempotencyKeyReader, IdempotencyKeyReader>();
            services.AddSingleton(new RedisManagerPool(configuration.GetConnectionString("Redis")));
            services.AddScoped(provider =>
            {
                var manager = provider.GetService<RedisManagerPool>();
                return manager.GetClient();
            });
            services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
        }

        public static void UseIdempotency(this IApplicationBuilder app)
        {
            app.UseMiddleware<IdempotencyMiddleware>();
        }
    }
}