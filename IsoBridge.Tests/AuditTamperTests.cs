using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IsoBridge.Core.Audit;
using IsoBridge.Infrastructure.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IsoBridge.Tests
{
    public class AuditTamperTests
    {
        private static AuditDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AuditDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AuditDbContext(options);
        }

        [Fact]
        public async Task VerifyAsync_Should_Return_True_For_Valid_Chain()
        {
            var db = CreateInMemoryDb();
            var hasher = new Sha256AuditHasher(Options.Create(new AuditSecurityOptions { HmacKey = "unit-test-key" }));
            var repo = new AuditRepository(db, hasher);
            var verifier = new AuditVerifier(db, hasher);

            await repo.AppendAsync(new AuditEntry
            {
                Actor = "service-A",
                Service = "parser",
                RequestDigest = "req1",
                ResponseDigest = "res1",
                MetaJson = "{}"
            });

            await repo.AppendAsync(new AuditEntry
            {
                Actor = "service-B",
                Service = "api",
                RequestDigest = "req2",
                ResponseDigest = "res2",
                MetaJson = "{}"
            });

            var result = await verifier.VerifyAsync();

            Assert.True(result.IsValid);
            Assert.Contains("verified", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task VerifyAsync_Should_Fail_When_Hash_Chain_Tampered()
        {
            var db = CreateInMemoryDb();
            var hasher = new Sha256AuditHasher(Options.Create(new AuditSecurityOptions { HmacKey = "unit-test-key" }));
            var repo = new AuditRepository(db, hasher);
            var verifier = new AuditVerifier(db, hasher);

            await repo.AppendAsync(new AuditEntry
            {
                Actor = "svc1",
                Service = "core",
                RequestDigest = "r1",
                ResponseDigest = "s1",
                MetaJson = "{}"
            });

            await repo.AppendAsync(new AuditEntry
            {
                Actor = "svc2",
                Service = "iso",
                RequestDigest = "r2",
                ResponseDigest = "s2",
                MetaJson = "{}"
            });

            // Tamper: break PrevHash of the latest entry
            var last = db.AuditEntries.OrderByDescending(x => x.TimestampUtc).First();
            last.PrevHash = "DEADBEEF";
            db.Update(last);
            await db.SaveChangesAsync();

            var result = await verifier.VerifyAsync();

            Assert.False(result.IsValid);
        }

        [Fact]
        public void ComputeHmac_Should_Differ_For_Different_Keys()
        {
            var hasherA = new Sha256AuditHasher(Options.Create(new AuditSecurityOptions { HmacKey = "A" }));
            var hasherB = new Sha256AuditHasher(Options.Create(new AuditSecurityOptions { HmacKey = "B" }));

            var hash = "abc123";
            var hmacA = hasherA.ComputeHmac(hash);
            var hmacB = hasherB.ComputeHmac(hash);

            Assert.NotEqual(hmacA, hmacB);
        }
    }
}