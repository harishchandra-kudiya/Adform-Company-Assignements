using Adfrom_CurrencyConversion.Interfaces;
using Adfrom_CurrencyConversion.Models;
using log4net;
using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;

namespace Adfrom_CurrencyConversion.Services
{
    public class CurrencyService : ICurrencyService
    {
        private const string FilePath = "Shared/currencyRates.json"; // Path to the JSON file containing currency rates
        private const string ApiUrl = @"https://www.nationalbanken.dk/api/currencyratesxml?lang=en"; // URL to the Nationalbanken API
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CurrencyService));

        private readonly HttpClient _httpClient;

        // This constructor injects an HttpClient instance to be used for fetching data from the API.
        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Fetches the latest currency exchange rates from the Nationalbanken API, 
        /// parses the XML response, and saves the rates into a JSON file.
        /// If an error occurs during fetching or processing, it logs the error and rethrows the exception.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task FetchAndSaveLatestRatesAsync()
        {
            try 
            {
                 _logger.Info("Fetching latest currency rates...");
                var response = await _httpClient.GetStringAsync(ApiUrl); // Fetch rates from API
                var rates = ParseXml(response); // Parse the XML response
                await SaveRatesToFileAsync(rates); // Save the rates to a file
                _logger.Info("Successfully fetched and saved currency rates.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error while fetching currency rates", ex);
                throw;
            }
           
        }

        /// <summary>
        /// Parses the XML response from the currency exchange rate API and converts all rates relative to INR.
        /// If the INR exchange rate is not found, an exception is thrown.
        /// Additionally, the Danish Krone (DKK) is manually added with its rate as INR per DKK.
        /// </summary>
        /// <param name="xmlContent">The XML response containing exchange rate data.</param>
        /// <returns>A list of <see cref="CurrencyRate"/> objects with rates relative to INR.</returns>
        /// <exception cref="Exception">Thrown if the INR rate is not found in the XML.</exception>
        private List<CurrencyRate> ParseXml(string xmlContent)
        {
            try 
            {
                _logger.Info("Parsing an XML content...");
                var doc = XDocument.Parse(xmlContent); // Parse the XML content

                // Find INR rate
                var inrRateElement = doc.Descendants("currency")
                                        .FirstOrDefault(x => x.Attribute("code")?.Value == "INR") ?? throw new Exception("INR rate not found in the exchange rates."); // Throw exception if INR rate is not found

                decimal inrRate = decimal.Parse(inrRateElement.Attribute("rate").Value, CultureInfo.InvariantCulture); // Parse the INR rate

                _logger.Info($"Base currency changed from DKK to INR. INR Rate: {inrRate}");

                // Convert all rates to be relative to INR
                var currencyRates = doc.Descendants("currency")
                    .Select(x => new CurrencyRate
                    {
                        CurrencyCode = x.Attribute("code").Value,
                        CurrencyDesc = x.Attribute("desc").Value,
                        Rate = Math.Round(decimal.Parse(x.Attribute("rate").Value, CultureInfo.InvariantCulture) / inrRate , 2),
                        DateTime = DateTime.Now.ToString("yyyy-MMM-dd hh:mm:ss:ff tt")
                    }).ToList();

                // Adding DKK manually with its rate as INR per DKK
                currencyRates.Add(new CurrencyRate
                {
                    CurrencyCode = "DKK",
                    CurrencyDesc = "Danish Krone",
                    Rate = inrRate,
                    DateTime = DateTime.Now.ToString("yyyy-MMM-dd hh:mm:ss:ff tt")
                });

                return currencyRates;
            }
            catch (Exception ex)
            {
                _logger.Error("Error while parsing XML", ex);
                throw;
            }
        }

        /// <summary>
        /// Saves the fetched currency exchange rates to a JSON file.
        /// Ensures that the target directory exists before writing the file.
        /// If an error occurs during the process, it is logged and the exception is thrown.
        /// </summary>
        /// <param name="rates">The list of currency exchange rates to be saved.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        /// <exception cref="Exception">Thrown if an error occurs while writing to the file.</exception>
        private async Task SaveRatesToFileAsync(List<CurrencyRate> rates)
        {
            try
            {
                _logger.Info("Saving currency rates to file...");
                var json = JsonSerializer.Serialize(rates, new JsonSerializerOptions { WriteIndented = true }); // Serialize the rates to JSON

                // Ensure directory exists
                var directory = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Overwrite the file
                await File.WriteAllTextAsync(FilePath, json);

                _logger.Info("Successfully saved currency rates to file.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error while saving currency rates to file", ex);
                throw;
            }
        }

        /// <summary>
        /// Loads the currency exchange rates from a JSON file.
        /// If the file does not exist, it fetches the latest rates from the API and saves them before loading.
        /// Logs errors if any issues occur during file reading or deserialization.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, returning a list of currency exchange rates.</returns>
        /// <exception cref="Exception">Thrown if an error occurs while reading or deserializing the file.</exception>
        public async Task<List<CurrencyRate>> LoadRatesFromFileAsync()
        {
            try
            {
                _logger.Info("Loading currency rates from file...");
                if (!File.Exists(FilePath))
                {
                    await FetchAndSaveLatestRatesAsync(); // Fetch and save if file doesn't exist
                }
                var json = await File.ReadAllTextAsync(FilePath); // Read the file
                return JsonSerializer.Deserialize<List<CurrencyRate>>(json); // Deserialize the JSON
            }
            catch (Exception ex)
            {
                _logger.Error("Error while loading currency rates from file", ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the exchange rate of a specified currency against the base currency (INR).
        /// Loads exchange rates from the local JSON file and finds the rate for the requested currency.
        /// </summary>
        /// <param name="currencyCode">The currency code to retrieve the exchange rate for.</param>
        /// <returns>A task representing the asynchronous operation, returning a <see cref="CurrencyRateResponse"/> containing the exchange rate.</returns>
        /// <exception cref="ArgumentException">Thrown if the provided currency code is not found in the exchange rates.</exception>
        public async Task<CurrencyRateResponse> GetCurrencyRateAsync(string currencyCode)
        {
            var rates = await LoadRatesFromFileAsync(); // Load exchange rates from file

            // Find the requested currency rate
            var currencyRate = rates.FirstOrDefault(r => r.CurrencyCode == currencyCode) ?? throw new ArgumentException("Invalid currency code."); // Throw exception if currency code is invalid

            return new CurrencyRateResponse
            {
                BaseCurrencyCode = "INR",
                BaseCurrencyAmount = 1,
                ConvertedCurrencyCode = currencyCode,
                ConvertedCurrencyAmount = Math.Round(currencyRate.Rate, 2)
            };
        }

        /// <summary>
        /// Converts a given amount from one currency to another based on the latest exchange rates.
        /// If the source and destination currencies are the same, the original amount is returned.
        /// </summary>
        /// <param name="request">A <see cref="CurrencyConversionRequest"/> containing the source currency, target currency, and amount.</param>
        /// <returns>A task representing the asynchronous operation, returning a <see cref="CurrencyConversionResponse"/> containing the converted amount.</returns>
        /// <exception cref="ArgumentException">Thrown if an invalid currency code is provided.</exception>
        /// <exception cref="Exception">Thrown if an unexpected error occurs during the conversion process.</exception>
        public async Task<CurrencyConversionResponse> ConvertCurrencyAsync(CurrencyConversionRequest request)
        {
            try
            {
                _logger.Info("Converting currency...");

                // If the source and destination currencies are the same, return the original amount
                if (request.FromCurrencyCode == request.ToCurrencyCode)
                {
                    return new CurrencyConversionResponse
                    {
                        FromCurrencyCode = request.FromCurrencyCode,
                        FromCurrencyDesc = "Same currency",
                        ToCurrencyCode = request.ToCurrencyCode,
                        ToCurrencyDesc = "Same currency",
                        OriginalAmount = request.Amount,
                        ConvertedAmount = request.Amount
                    };
                }

                // Load exchange rates from file
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
            catch (Exception ex)
            {
                _logger.Error("Error while converting currency", ex);
                throw;
            }
        }

    }
}
