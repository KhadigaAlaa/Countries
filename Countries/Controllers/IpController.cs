using Countries.Models;
using Countries.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Countries.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IpController : ControllerBase
    {
        private readonly IpLookupService _ipLookupService;
        private readonly BlockCountryService _blockService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BlockLogService _logService;

        public IpController(
            IpLookupService ipLookupService,
            BlockCountryService blockService,
            BlockLogService logService,
            IHttpContextAccessor httpContextAccessor)
        {
            _ipLookupService = ipLookupService;
            _blockService = blockService;
            _logService = logService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> LookupIp([FromQuery] string? ipAddress)
        {
            ipAddress ??= _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress)) return BadRequest("Invalid IP address.");

            var countryCode = await _ipLookupService.GetCountryCodeFromIpAsync(ipAddress);
            return countryCode != null ? Ok(new { ipAddress, countryCode }) : NotFound("Could not retrieve country.");
        }

        [HttpGet("check-block2")]
        public async Task<IActionResult> CheckIpBlock2([FromQuery] string? ipAddress)
        {
            ipAddress ??= _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(ipAddress))
            {
                return BadRequest("Invalid IP address.");
            }

            // using the third-party 
            var countryCode = await _ipLookupService.GetCountryCodeFromIpAsync(ipAddress);
            if (countryCode == null)
            {
                return NotFound("Could not determine country.");
            }

            var isBlocked = _blockService.GetBlockedCountries().Contains(countryCode);

            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
            _logService.LogBlockedAttempt(ipAddress, countryCode, isBlocked, userAgent);

            return Ok(new
            {
                ipAddress,
                countryCode,
                isBlocked
            });
        }

        [HttpGet("check-block")]
        public async Task<IActionResult> CheckIpBlock()
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress))
            {
                return BadRequest("Invalid IP address.");
            }

            var countryCode = await _ipLookupService.GetCountryCodeFromIpAsync(ipAddress);
            if (countryCode == null)
            {
                return NotFound("Could not determine country.");
            }

            var isBlocked = _blockService.GetBlockedCountries().Contains(countryCode);

            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
            _logService.LogBlockedAttempt(ipAddress, countryCode, isBlocked, userAgent);

            return Ok(new
            {
                ipAddress,
                countryCode,
                isBlocked
            });
        }


        [HttpGet("logs/blocked-attempts")]
        public IActionResult GetBlockedAttempts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var logs = _logService.GetBlockedAttemptLogs(page, pageSize);
            return Ok(logs);
        }

    
        [HttpPost("temporal-block")]
        public IActionResult TemporalBlockCountry([FromBody] TemporalBlockRequest request)
        {
            if (request.DurationMinutes < 1 || request.DurationMinutes > 1440)
            {
                return BadRequest("Duration must be between 1 and 1440 minutes.");
            }

            if (string.IsNullOrEmpty(request.CountryCode) || request.CountryCode.Length != 2)
            {
                return BadRequest("Invalid country code.");
            }

            // Attempt to temporarily block the country
            var success = _blockService.TemporarilyBlockCountry(request.CountryCode, request.DurationMinutes);
            if (success)
            {
                return Ok($"Country {request.CountryCode} temporarily blocked for {request.DurationMinutes} minutes.");
            }
            else
            {
                return Conflict("Country is already temporarily blocked.");
            }
        }

        


    }
}
