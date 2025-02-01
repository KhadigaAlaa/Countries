using Countries.Services;
using Microsoft.AspNetCore.Mvc;

namespace Countries.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlockedCountryController : ControllerBase
    {
        private readonly BlockCountryService _blockService;
        public BlockedCountryController(BlockCountryService blockService)
        {
            _blockService = blockService;
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
  

    }
}
