using Countries.Models;
using System.Collections.Concurrent;

namespace Countries.Services.BackJobWithHangfire
{
    public interface ITempBlockHangfire
    {
        public void RemoveingTemp(ConcurrentDictionary<string, bool> _blockedCountries, ConcurrentDictionary<string, TemporalBlockedCountry> _temporalBlockedCountries, string CountryCode);
        public void RemoveingTemp(string countryCode);

    }
}
