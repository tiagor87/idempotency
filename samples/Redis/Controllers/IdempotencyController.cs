using System.Threading;
using Microsoft.AspNetCore.Mvc;

namespace Idempotency.Redis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdempotencyController : ControllerBase
    {
        private static int _calls;

        [HttpPost]
        public IActionResult Post()
        {
            Interlocked.Increment(ref _calls);
            return StatusCode(201, _calls);
        }

        [HttpPatch]
        public IActionResult Patch()
        {
            Interlocked.Increment(ref _calls);
            return Ok(_calls);
        }

        [HttpPut]
        public IActionResult Put()
        {
            Interlocked.Increment(ref _calls);
            return Ok(_calls);
        }

        [HttpDelete]
        public IActionResult Delete()
        {
            Interlocked.Increment(ref _calls);
            return NoContent();
        }

        [HttpGet]
        public IActionResult Get(string key)
        {
            Interlocked.Increment(ref _calls);
            return Ok(_calls);
        }
    }
}