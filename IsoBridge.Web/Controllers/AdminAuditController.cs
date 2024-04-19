using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IsoBridge.Infrastructure.Audit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IsoBridge.Web.Controllers
{
    [Route("admin/audit")]
    public class AdminAuditController : Controller
    {
        private readonly AuditDbContext _db;
        private readonly AuditVerifier _verifier;

        public AdminAuditController(AuditDbContext db, AuditVerifier verifier)
        {
            _db = db;
            _verifier = verifier;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var entries = await _db.AuditEntries
                .AsNoTracking()
                .OrderByDescending(a => a.TimestampUtc)
                .ToListAsync();

            return View("Index", entries);
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify()
        {
            var result = await _verifier.VerifyAsync();
            TempData["VerifyMessage"] = result.Message;
            TempData["VerifyIsValid"] = result.IsValid;

            var entries = await _db.AuditEntries
                .AsNoTracking()
                .OrderByDescending(a => a.TimestampUtc)
                .ToListAsync();

            return View("Index", entries);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportCsv()
        {
            var entries = await _db.AuditEntries.AsNoTracking().OrderBy(a => a.TimestampUtc).ToListAsync();
            var csv = string.Join(Environment.NewLine,
                new[] { "TimestampUtc,Actor,Service,Hash,HmacSignature" }
                .Concat(entries.Select(e =>
                    $"{e.TimestampUtc:o},{e.Actor},{e.Service},{e.Hash},{e.HmacSignature}")));

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "audit-log.csv");
        }
    }
}