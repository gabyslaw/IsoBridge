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
    public class AuditVerifierTests
    {
        private readonly AuditDbContext _db;
        private readonly Sha256AuditHasher _hasher;
        private readonly AuditRepository _repo;
        private readonly AuditVerifier _verifier;

        public AuditVerifierTests()
        {
            var opts = new DbContextOptionsBuilder<AuditDbContext>()
                .UseInMemoryDatabase("AuditVerifyDb")
                .Options;
            _db = new AuditDbContext(opts);
            _hasher = new Sha256AuditHasher(Options.Create(new AuditSecurityOptions { HmacKey = "verify-key" }));
            _repo = new AuditRepository(_db, _hasher);
            _verifier = new AuditVerifier(_db, _hasher);
        }

        [Fact]
        public async Task VerifyAsync_Should_Return_True_For_Valid_Chain()
        {
            await _repo.AppendAsync(new AuditEntry
            {
                Actor = "alpha",
                Service = "svc",
                RequestDigest = "A",
                ResponseDigest = "B"
            });
            await _repo.AppendAsync(new AuditEntry
            {
                Actor = "beta",
                Service = "svc",
                RequestDigest = "C",
                ResponseDigest = "D"
            });

            var result = await _verifier.VerifyAsync();

            Assert.True(result.IsValid);
            Assert.Equal("Chain verified successfully.", result.Message);
        }

        [Fact]
        public async Task VerifyAsync_Should_Fail_When_Hash_Tampered()
        {
            await _repo.AppendAsync(new AuditEntry
            {
                Actor = "tamper",
                Service = "svc",
                RequestDigest = "E",
                ResponseDigest = "F"
            });

            var first = await _db.AuditEntries.FirstAsync();
            first.Hash = "DEADBEEF"; // simulating tampering here
            _db.Update(first);
            await _db.SaveChangesAsync();

            var result = await _verifier.VerifyAsync();

            Assert.False(result.IsValid);
        }
    }
}