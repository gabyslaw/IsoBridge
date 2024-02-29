using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IsoBridge.Core.Audit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IsoBridge.Infrastructure.Audit
{
    public sealed class AuditSecurityOptions { public string HmacKey { get; set; } = "dev-secret-change"; }

    public sealed class Sha256AuditHasher : IAuditHasher
    {
        private readonly AuditSecurityOptions _opt;
        public Sha256AuditHasher(IOptions<AuditSecurityOptions> opt) => _opt = opt.Value;

        public string ComputeHash(string prevHash, DateTime tsUtc, string req, string res, string metaJson)
        {
            var payload = $"{prevHash}|{tsUtc:O}|{req}|{res}|{metaJson}";
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(payload)));
        }

        public string ComputeHmac(string hash)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_opt.HmacKey));
            return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(hash)));
        }
    }

    public static class InfraServiceCollectionExtensions
    {
        public static IServiceCollection AddIsoBridgeInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<AuditSecurityOptions>(config.GetSection("AuditSecurity"));
            services.AddSingleton<IAuditHasher, Sha256AuditHasher>();
            // I will add EF Core context later
            return services;
        }
    }
}