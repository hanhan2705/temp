using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize(Policy = "HR_OR_ADMIN")]
    [Route("api/v1/history")]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        [HttpGet("allocations")]
        public async Task<IActionResult> GetAllocations()
        {
            var data = await _historyService.GetAllocationsAsync();
            return Ok(new { data });
        }

        [HttpGet("recoveries")]
        public async Task<IActionResult> GetRecoveries()
        {
            var data = await _historyService.GetRecoveriesAsync();
            return Ok(new { data });
        }

        [HttpGet("activity")]
        public async Task<IActionResult> GetActivity()
        {
            var data = await _historyService.GetActivityAsync();
            return Ok(new { data });
        }
    }
}
