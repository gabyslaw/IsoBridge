using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IsoBridge.Core.ISO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IsoBridge.ISO8583
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIso8583(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<Iso8583Options>(config.GetSection("Iso8583"));
            services.AddSingleton<IIsoParser, DefaultIsoParser>();
            return services;
        }
    }
}