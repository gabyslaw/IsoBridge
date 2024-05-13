using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IsoBridge.Adapters.Forwarding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace IsoBridge.Adapters
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIsoBridgeAdapters(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<ForwardingOptions>(config.GetSection("Forwarding"));

            // HttpClient for forwarder with Polly retry/backoff
            services.AddHttpClient("IsoBridge.Forwarder")
                .AddPolicyHandler(CreateRetryPolicy());

            services.AddSingleton<IForwarder, RestForwarder>();
            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(r => r.StatusCode == HttpStatusCode.RequestTimeout)
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(200 * attempt));
        }
    }
}