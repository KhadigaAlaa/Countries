using Countries.Services;
using Microsoft.AspNetCore.Mvc;

namespace Countries.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlockedCountryController : Controller
    {
        private readonly BlockCountryService _blockService;
        private readonly BlockLogService _blockLogService;

        public BlockedCountryController(BlockCountryService blockService, BlockLogService blockLogService)
        {
            _blockService = blockService;
            _blockLogService = blockLogService; 
        }

        [HttpPost("block")]
        public IActionResult BlockCountry([FromBody] string countryCode)
        {
            if (_blockService.BlockCountry(countryCode)) return Ok("Country blocked.");
            return Conflict("Country already blocked.");
        }

        [HttpDelete("block/{countryCode}")]
        public IActionResult UnblockCountry(string countryCode)
        {
            if (_blockService.UnblockCountry(countryCode)) return Ok("Country unblocked.");
            return NotFound("Country not found in block list.");
        }

        [HttpGet("blocked")]
        public IActionResult GetBlockedCountries() => Ok(_blockService.GetBlockedCountries());

        [HttpGet("logs")]
        public IActionResult GetBlockedAttempts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? countryCode = null,
            [FromQuery] DateTime? fromDate = null,  
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? searchIp = null)
        {
            var logs = _blockLogService.GetBlockedAttemptLogs(page, pageSize, countryCode, fromDate, toDate, searchIp);
            return Ok(logs);
        }


    }
}
