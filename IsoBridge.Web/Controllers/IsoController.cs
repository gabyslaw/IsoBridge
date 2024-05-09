using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IsoBridge.Core.ISO;
using IsoBridge.Core.Models;
using IsoBridge.Web.Models;
using IsoBridge.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IsoBridge.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IsoController : ControllerBase
    {
        private readonly IIsoParser _parser;
        private readonly AuditLoggingService _audit;

        public IsoController(IIsoParser parser, AuditLoggingService audit)
        {
            _parser = parser;
            _audit = audit;
        }

        [HttpPost("parse")]
        public async Task<ActionResult<IsoResponse>> Parse([FromBody] ParseIsoRequest request)
        {
            try
            {
                var bytes = request.Encoding.ToLower() switch
                {
                    "base64" => Convert.FromBase64String(request.Payload),
                    _ => Convert.FromHexString(request.Payload)
                };

                var result = _parser.Parse(bytes);
                await _audit.LogAsync("api-user", "parse", request.Payload, "ok", "{}");

                return Ok(new IsoResponse
                {
                    Mti = result.Message?.Mti ?? "????",
                    Fields = result.Message?.Fields,
                    Message = result.Success ? "Parsed successfully" : "Parse failed"
                });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("api-user", "parse", request.Payload, ex.Message, "{}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("build")]
        public async Task<ActionResult<IsoResponse>> Build([FromBody] BuildIsoRequest request)
        {
            try
            {
                var message = new IsoMessage(request.Mti, request.Fields);
                var bytes = _parser.Build(message);

                var hex = Convert.ToHexString(bytes);
                var b64 = Convert.ToBase64String(bytes);

                await _audit.LogAsync("api-user", "build", request.Mti, "ok", "{}");

                return Ok(new IsoResponse
                {
                    Mti = request.Mti,
                    Fields = request.Fields,
                    Hex = hex,
                    Base64 = b64,
                    Message = "Built successfully"
                });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("api-user", "build", request.Mti, ex.Message, "{}");
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}