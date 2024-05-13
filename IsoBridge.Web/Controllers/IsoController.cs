using System;
using System.Collections.Generic;
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
        private readonly ForwardingService _forwarding;

        public IsoController(IIsoParser parser, AuditLoggingService audit, ForwardingService forwarding)
        {
            _parser = parser;
            _audit = audit;
            _forwarding = forwarding;
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
                var success = result.Success;

                await _audit.LogAsync(
                    "api-user",
                    success ? "Parse" : "ParseError",
                    request.Payload,
                    success ? "Parsed OK" : result.Error ?? "Parse failed",
                    "{}"
                );

                return Ok(new IsoResponse
                {
                    Mti = result.Message?.Mti ?? "????",
                    Fields = result.Message?.Fields != null
                        ? new Dictionary<int, string>(result.Message.Fields)
                        : new Dictionary<int, string>(),
                    Message = success ? "Parsed successfully" : "Parse failed"
                });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("api-user", "ParseError", request.Payload, ex.Message, "{}");
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
                var base64 = Convert.ToBase64String(bytes);

                await _audit.LogAsync(
                    "api-user",
                    "Build",
                    request.Mti,
                    hex,
                    "{}"
                );

                return Ok(new IsoResponse
                {
                    Mti = request.Mti,
                    Fields = request.Fields,
                    Hex = hex,
                    Base64 = base64,
                    Message = "Built successfully"
                });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("api-user", "BuildError", request.Mti, ex.Message, "{}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Parse/build an ISO message and forward it to a configured upstream (REST for now).
        /// </summary>
        [HttpPost("forward")]
        [RequestSizeLimit(1024 * 256)] // 256 KB
        public async Task<IActionResult> Forward([FromBody] ForwardIsoRequest request, CancellationToken ct)
        {
            var (isoBytes, mappedJson, ok, err) =
                _forwarding.BuildIsoAndMapJson(request.Mode, request.Encoding, request.Payload, request.Mti, request.Fields);

            if (!ok)
            {
                await _audit.LogAsync("api-user", "ForwardError", request.Payload ?? request.Mti ?? "N/A", err ?? "error", "{}");
                return BadRequest(new { error = err });
            }

            try
            {
                var result = await _forwarding.ForwardAsync(request.RouteKey, isoBytes, mappedJson, ct);

                var meta = new Dictionary<string, object?>
                {
                    ["routeKey"] = request.RouteKey,
                    ["status"] = result.StatusCode
                };
                await _audit.LogAsync("api-user", "Forward", Convert.ToHexString(isoBytes), result.Body, System.Text.Json.JsonSerializer.Serialize(meta));

                return StatusCode(result.StatusCode, new
                {
                    upstreamStatus = result.StatusCode,
                    upstreamBody = result.Body,
                    upstreamHeaders = result.Headers
                });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("api-user", "ForwardError", Convert.ToHexString(isoBytes), ex.Message, "{}");
                return StatusCode(502, new { error = "Upstream call failed", detail = ex.Message });
            }
        }
    }
}
