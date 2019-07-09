using System.Linq;
using System.Net;
using System.Reflection;
using Idempotency.Core;
using Idempotency.Redis;
using Idempotency.Samples.Redis.Core.Commands;
using Idempotency.Samples.Redis.Core.Serializers;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Newtonsoft.Json;

namespace Idempotency.Samples.Redis
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
            services.AddRedisIdempotency(Configuration);
            services.AddRedisBehaviorPipeline(Configuration);
            services.AddScoped<IIdempotencyKeyReader<IncrementCounter>, IncrementCounterKeyReader>();
            services.AddScoped<IIdempotencySerializer, NewtonsoftIdempotencySerializer>();
            
            var runtimeId = RuntimeEnvironment.GetRuntimeIdentifier();
            var assemblies = DependencyContext.Default.GetRuntimeAssemblyNames(runtimeId)
                .Select(Assembly.Load).ToArray();
            services.AddMediatR(assemblies);
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
            app.UseIdempotency();
            app.UseMvc();
        }
    }
}