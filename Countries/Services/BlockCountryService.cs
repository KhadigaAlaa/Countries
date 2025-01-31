using Countries.Models;
using Countries.Services.BackJobWithHangfire;
using Hangfire;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace Countries.Services
{
    public class BlockCountryService
    {

        private readonly ConcurrentDictionary<string, bool> _blockedCountries = new();
        private readonly ConcurrentDictionary<string, TemporalBlockedCountry> _temporalBlockedCountries1 = new();

        public bool BlockCountry(string countryCode) => _blockedCountries.TryAdd(countryCode, true);
        public bool UnblockCountry(string countryCode)
        {
            bool removed = _blockedCountries.TryRemove(countryCode, out _);
            _temporalBlockedCountries1.TryRemove(countryCode, out _); 
            return removed;
        }

        public IEnumerable<string> GetBlockedCountries() => _blockedCountries.Keys;

        public void RemoveFromTemporaryBlockedCountries(string countryCode)
        {
            _temporalBlockedCountries1.TryRemove(countryCode, out _);
        }


        public bool TemporarilyBlockCountry(string countryCode, int durationMinutes)
        {
            if (durationMinutes < 1 || durationMinutes > 1440) return false; // Validate input duration

            if (_temporalBlockedCountries1.ContainsKey(countryCode) || _blockedCountries.ContainsKey(countryCode))
                return false; // Prevent duplicates

            var expirationTime = DateTime.UtcNow.AddMinutes(durationMinutes);
            var temporalBlock = new TemporalBlockedCountry
            {
                CountryCode = countryCode,
                BlockedUntil = expirationTime
            };

            var isAdded = _temporalBlockedCountries1.TryAdd(countryCode, temporalBlock);

            if (isAdded)
            {
                _blockedCountries.TryAdd(countryCode, true);

                //Schedule automatic removal using Hangfire
                BackgroundJob.Schedule<ITempBlockHangfire>(
                    s => s.RemoveingTemp(_blockedCountries, _temporalBlockedCountries1, countryCode),
                    TimeSpan.FromMinutes(durationMinutes)
                );

                return true;
            }
            return false;
        }

        //Removes all expired temporary blocks.
        public void RemoveExpiredTemporaryBlocks()
        {
            var expiredBlocks = _temporalBlockedCountries1
                .Where(kvp => kvp.Value.BlockedUntil <= DateTime.UtcNow)
                .Select(kvp => kvp.Key)
                .ToList(); // Get expired country codes

            foreach (var countryCode in expiredBlocks)
            {
                UnblockCountry(countryCode);
            }
        }
    }
}
