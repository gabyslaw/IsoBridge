using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IsoBridge.Adapters.Forwarding;
using Polly;
using Polly.Extensions.Http;

namespace IsoBridge.Adapters
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIsoBridgeAdapters(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<ForwardingOptions>(config.GetSection("Forwarding"));

            // Named HttpClient with Polly retry/backoff
            services.AddHttpClient("IsoBridge.Forwarder")
                .AddPolicyHandler(CreateRetryPolicy());

            // register concrete forwarders + router
            services.AddSingleton<RestForwarder>();
            services.AddSingleton<SoapForwarder>();
            services.AddSingleton<IForwarder, ForwarderRouter>();

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
