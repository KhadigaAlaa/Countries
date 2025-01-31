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

        public IEnumerable<BlockedAttemptLog> GetBlockedAttemptLogs(
            int page, int pageSize, string? countryCode = null, DateTime? fromDate = null,
            DateTime? toDate = null, string? searchIp = null)
        {
            var query = _blockedAttemptLogs.AsQueryable();

            // Filtering by Country Code
            if (!string.IsNullOrEmpty(countryCode))
                query = query.Where(log => log.CountryCode == countryCode);

            // Filtering by Date Range
            if (fromDate.HasValue)
                query = query.Where(log => log.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(log => log.Timestamp <= toDate.Value);

            if (!string.IsNullOrEmpty(searchIp))
                query = query.Where(log => log.IpAddress.Contains(searchIp));

            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }
    }
}
