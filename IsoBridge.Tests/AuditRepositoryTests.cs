using IsoBridge.Core.Audit;
using IsoBridge.Infrastructure.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace IsoBridge.Tests
{
    public class AuditRepositoryTests
    {
        private readonly AuditRepository _repo;
        private readonly AuditDbContext _db;

        public AuditRepositoryTests()
        {
            var opts = new DbContextOptionsBuilder<AuditDbContext>()
                .UseInMemoryDatabase("AuditRepoTestDb")
                .Options;

            _db = new AuditDbContext(opts);
            var hasher = new Sha256AuditHasher(Options.Create(new AuditSecurityOptions { HmacKey = "test-key" }));
            _repo = new AuditRepository(_db, hasher);
        }

        [Fact]
        public async Task AppendAsync_Should_Add_New_Entry_With_Hash_Chain()
        {
            var entry = new AuditEntry
            {
                Actor = "tester",
                Service = "unit",
                RequestDigest = "REQ",
                ResponseDigest = "RESP"
            };

            await _repo.AppendAsync(entry);

            var stored = await _db.AuditEntries.FirstAsync();
            Assert.False(string.IsNullOrWhiteSpace(stored.Hash));
            Assert.False(string.IsNullOrWhiteSpace(stored.HmacSignature));
        }
    }
}