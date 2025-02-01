using System.Text.Json;

namespace Countries.Services
{
    public class IPLookupService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public IPLookupService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task<string?> GetCountryCodeFromIpAsync(string ipAddress)
        {
            string apiKey = _configuration["IpGeoLocationSettings:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("API key is missing!");
                return null;
            }

            string url = $"https://api.ipgeolocation.io/ipgeo?apiKey={apiKey}&ip={ipAddress}";

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                using var jsonDoc = JsonDocument.Parse(response);

                if (jsonDoc.RootElement.TryGetProperty("country_code2", out var countryCodeElement))
                {
                    return countryCodeElement.GetString();
                }
                else
                {
                    Console.WriteLine("Country code not found in the response.");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request failed: {ex.Message}");
                return ex.Message;
            }
        }
    }

}
