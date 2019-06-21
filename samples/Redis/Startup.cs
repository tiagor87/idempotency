using System.Net;
using Idempotency.Core;
using Idempotency.Redis.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ServiceStack.Redis;

namespace Idempotency.Redis
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSingleton(new RedisManagerPool(Configuration.GetConnectionString("Redis")));
            services.AddScoped(provider =>
            {
                var manager = provider.GetService<RedisManagerPool>();
                return manager.GetClient();
            });
            services.AddScoped<IIdempotencyRepository, RedisRepository>();
            services.AddScoped<IIdempotencyKeyReader, IdempotencyKeyReader>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMiddleware<IdempotencyMiddleware>();
            app.UseMvc();
            app.UseExceptionHandler(new ExceptionHandlerOptions()
            {
                ExceptionHandler = async context =>
                {
                    context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var ex = context.Features.Get<IExceptionHandlerFeature>();
                    if (ex != null)
                    {
                        await context.Response.WriteAsync(
                            JsonConvert.SerializeObject(ex.Error.Message));
                    }
                }
            });
        }
    }
}