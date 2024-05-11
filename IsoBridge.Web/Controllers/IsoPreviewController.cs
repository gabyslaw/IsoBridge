using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IsoBridge.Infrastructure.Audit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IsoBridge.Web.Controllers
{
    [Route("admin/iso-preview")]
    public class IsoPreviewController : Controller
    {
        private readonly AuditDbContext _db;

        public IsoPreviewController(AuditDbContext db)
        {
            _db = db;
        }

        [HttpGet("")]
        [HttpGet("index")]
        public IActionResult Index() => View();

        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics([FromServices] AuditDbContext db)
        {
            var totalBuilds = await db.AuditEntries.CountAsync(a => a.Service.Contains("Build"));
            var totalParses = await db.AuditEntries.CountAsync(a => a.Service.Contains("Parse"));
            var totalErrors = await db.AuditEntries.CountAsync(a => a.Service.Contains("Error"));
            var lastOperation = await db.AuditEntries
                .OrderByDescending(a => a.TimestampUtc)
                .Select(a => a.Service)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                totalBuilds,
                totalParses,
                totalErrors,
                lastOperation = lastOperation ?? "None"
            });
        }
    }
}