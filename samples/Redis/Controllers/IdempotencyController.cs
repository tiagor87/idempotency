using System.Threading;
using System.Threading.Tasks;
using Idempotency.Samples.Redis.Core.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Idempotency.Samples.Redis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdempotencyController : ControllerBase
    {
        private readonly IMediator _mediator;
        private static int _calls;

        public IdempotencyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public IActionResult Post()
        {
            Interlocked.Increment(ref _calls);
            return StatusCode(201, _calls);
        }
        
        [HttpPost("command/{count}")]
        public async Task<IActionResult> Post(int count, [FromQuery] string key)
        {
            var calls = await _mediator.Send(new IncrementCounter(key, count));
            return StatusCode(201, calls);
        }

        [HttpPost("other")]
        public IActionResult PostOther()
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