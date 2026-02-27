using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Rahal.Api.Controllers._Common
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("per-user")]
    public class CustomControllerBase : ControllerBase
    {
    }
}
