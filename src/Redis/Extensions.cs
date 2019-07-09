using Idempotency.Core;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Idempotency.Redis
{
    public static class Extensions
    {
        public static IServiceCollection AddRedisIdempotency(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddRedis(configuration)    
                .AddScoped<IIdempotencyKeyReader<HttpRequest>, HttpRequestIdempotencyKeyReader>()
                .AddScoped<IIdempotencyRepository, IdempotencyRepository>()
                .AddScoped<IIdempotencySerializer, IdemportencySerializer>();
        }

        public static IApplicationBuilder UseIdempotency(this IApplicationBuilder app)
        {
            return app.UseMiddleware<IdempotencyMiddleware>();
        }

        public static IServiceCollection AddRedisBehaviorPipeline<TRequest, TResponse>(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddRedis(configuration)
                .AddScoped(typeof(IPipelineBehavior<TRequest, TResponse>), typeof(IdempotencyBehaviorPipeline<TRequest, TResponse>));
        }
        
        public static IServiceCollection AddRedisBehaviorPipeline(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddRedis(configuration)
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehaviorPipeline<,>));
        }

        private static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")))
                .AddScoped(provider =>
                {
                    var connection = provider.GetService<IConnectionMultiplexer>();
                    return connection.GetDatabase();
                });
        }
    }
}