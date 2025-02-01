using Countries.Models;
using System.Collections.Concurrent;

namespace Countries.Services
{
    public class BlockLogService
    {
        private readonly ConcurrentBag<BlockedAttemptLog> _blockedAttemptLogs = new();

        public void LogBlockedAttempt(string ipAddress, string countryCode, bool isBlocked, string userAgent)
        {
            _blockedAttemptLogs.Add(new BlockedAttemptLog
            {
                IpAddress = ipAddress,
                CountryCode = countryCode,
                IsBlocked = isBlocked,
                Timestamp = DateTime.UtcNow,
                UserAgent = userAgent
            });
        }

        public IEnumerable<BlockedAttemptLog> GetBlockedAttemptLogs(int page, int pageSize)
        {
            return _blockedAttemptLogs.Skip((page - 1) * pageSize).Take(pageSize);
        }
    }
}
