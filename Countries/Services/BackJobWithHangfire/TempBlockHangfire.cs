using IpAddress.Models;
using System.Collections.Concurrent;

namespace IpAddress.Services.BackJobWithHangfire
{
    public class TempBlockHangfire: ITempBlockHangfire
    {
        public void RemoveingTemp(ConcurrentDictionary<string, bool> _blockedCountries , ConcurrentDictionary<string, TemporalBlockedCountry> _temporalBlockedCountries , string CountryCode)
        {
            _blockedCountries.TryRemove(CountryCode, out _);
            _temporalBlockedCountries.TryRemove(CountryCode, out _);
            _blockCountryService.UnblockCountry(CountryCode); // Use the existing Unblock method

        }

        private readonly BlockCountryService _blockCountryService;

        public TempBlockHangfire(BlockCountryService blockCountryService)
        {
            _blockCountryService = blockCountryService;
        }

        public void RemoveingTemp(string countryCode)
        {

            _blockCountryService.UnblockCountry(countryCode);
            // Use the existing Unblock method
            _blockCountryService.RemoveFromTemporaryBlockedCountries(countryCode); // Also remove from _temporalBlockedCountries1

        }
    }
}
