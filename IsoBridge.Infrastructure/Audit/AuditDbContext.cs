using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IsoBridge.Core.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IsoBridge.Infrastructure.Audit
{
    public sealed class AuditDbContext : DbContext
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

        public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditEntry>(entity =>
            {
                entity.HasIndex(e => e.TimestampUtc);
                entity.Property(e => e.MetaJson).HasColumnType("TEXT");
                entity.Property(e => e.HmacSignature).IsRequired();
            });
        }

        public static void EnsureCreated(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
            db.Database.EnsureCreated();
        }

    }
}