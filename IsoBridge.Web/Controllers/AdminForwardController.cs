using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IsoBridge.Adapters.Forwarding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IsoBridge.Web.Controllers
{
    [Route("admin/forward")]
    public class AdminForwardController : Controller
    {
        private readonly IOptionsMonitor<ForwardingOptions> _forwardingOptions;

        public AdminForwardController(IOptionsMonitor<ForwardingOptions> forwardingOptions)
        {
            _forwardingOptions = forwardingOptions;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            // Pass configured routes to the view
            var routes = _forwardingOptions.CurrentValue.Routes.Keys.ToList();
            return View(routes);
        }
    }
}