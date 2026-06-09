using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisal.Application.Auth;
using Sisal.Application.Common.Messaging;

namespace SiSal.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MeController(IDispatcher dispatcher) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var me = await dispatcher.Query(new GetMeQuery(), cancellationToken);
            return Ok(me);
        }
    }
}
