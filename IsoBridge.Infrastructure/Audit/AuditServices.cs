using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IsoBridge.Core.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IsoBridge.Infrastructure.Audit
{
    public sealed class AuditSecurityOptions { public string HmacKey { get; set; } = "dev-secret-change"; }

    public sealed class Sha256AuditHasher : IAuditHasher
    {
        private readonly string _key;

        public Sha256AuditHasher(IOptions<AuditSecurityOptions> options)
        {
            _key = options.Value.HmacKey ?? string.Empty;
        }

        public string ComputeHash(AuditEntry entry, string prevHash)
        {
            var payload = $"{prevHash}|{entry.TimestampUtc:o}|{entry.RequestDigest}|{entry.ResponseDigest}|{entry.MetaJson}";
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(bytes);
        }

        public string ComputeHmac(string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_key));
            var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(bytes);
        }
    }

    public static class InfraServiceCollectionExtensions
    {
        public static IServiceCollection AddIsoBridgeInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<AuditSecurityOptions>(config.GetSection("AuditSecurity"));
            services.AddSingleton<IAuditHasher, Sha256AuditHasher>();

            services.AddDbContext<AuditDbContext>(options =>
                options.UseSqlite("Data Source=audit.db"));

            return services;
        }
    }
}