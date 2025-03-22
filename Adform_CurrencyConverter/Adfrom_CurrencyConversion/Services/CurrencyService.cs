using Adfrom_CurrencyConversion.Interfaces;
using Adfrom_CurrencyConversion.Models;
using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;

namespace Adfrom_CurrencyConversion.Services
{
    public class CurrencyService : ICurrencyService
    {
        private const string FilePath = "Shared/currencyRates.json";
        private const string ApiUrl = @"https://www.nationalbanken.dk/api/currencyratesxml?lang=en";

        private readonly HttpClient _httpClient;

        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Fetch the latest currency rates from API and save to JSON file
        public async Task FetchAndSaveLatestRatesAsync()
        {
            var response = await _httpClient.GetStringAsync(ApiUrl);
            var rates = ParseXml(response);
            await SaveRatesToFileAsync(rates);
        }


        // Parse XML API response
        private List<CurrencyRate> ParseXml(string xmlContent)
        {
            var doc = XDocument.Parse(xmlContent);
            return doc.Descendants("currency")
                .Select(x => new CurrencyRate
                {
                    CurrencyCode = x.Attribute("code").Value,
                    CurrencyDesc = x.Attribute("desc").Value,
                    Rate = decimal.Parse(x.Attribute("rate").Value, CultureInfo.InvariantCulture),
                    DateTime = DateTime.UtcNow
                }).ToList();
        }

        // Save fetched rates into JSON file
        private async Task SaveRatesToFileAsync(List<CurrencyRate> rates)
        {
            var json = JsonSerializer.Serialize(rates, new JsonSerializerOptions { WriteIndented = true });

            // Ensure directory exists
            var directory = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Overwrite the file
            await File.WriteAllTextAsync(FilePath, json);
        }


        public async Task<List<CurrencyRate>> LoadRatesFromFileAsync()
        {
            if (!File.Exists(FilePath))
            {
                await FetchAndSaveLatestRatesAsync(); // Fetch and save if file doesn't exist
            }

            var json = await File.ReadAllTextAsync(FilePath);
            return JsonSerializer.Deserialize<List<CurrencyRate>>(json);
        }

        public async Task<CurrencyConversionResponse> ConvertCurrencyAsync(CurrencyConversionRequest request)
        {
            var rates = await LoadRatesFromFileAsync();

            // Get exchange rates
            var fromRate = rates.FirstOrDefault(r => r.CurrencyCode == request.FromCurrencyCode);
            var toRate = rates.FirstOrDefault(r => r.CurrencyCode == request.ToCurrencyCode);

            if (fromRate == null || toRate == null)
            {
                throw new ArgumentException("Invalid currency code.");
            }

            // Convert to base currency first, then to the target currency
            decimal baseCurrencyAmount = request.Amount * fromRate.Rate;
            decimal convertedAmount = baseCurrencyAmount / toRate.Rate;

            return new CurrencyConversionResponse
            {
                FromCurrencyCode = request.FromCurrencyCode,
                FromCurrencyDesc = fromRate.CurrencyDesc,
                ToCurrencyCode = request.ToCurrencyCode,
                ToCurrencyDesc = toRate.CurrencyDesc,
                OriginalAmount = request.Amount,
                ConvertedAmount = Math.Round(convertedAmount, 2)
            };
        }

    }
}
