using Adfrom_CurrencyConversionDB.Data;
using Adfrom_CurrencyConversionDB.Interfaces;
using Adfrom_CurrencyConversionDB.Models;
using log4net;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;

namespace Adfrom_CurrencyConversionDB.Services
{
    public class CurrencyService : ICurrencyService
    {
        private const string ApiUrl = @"https://www.nationalbanken.dk/api/currencyratesxml?lang=en"; // URL to the Nationalbanken API
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CurrencyService));

        private readonly HttpClient _httpClient;
        private readonly CurrencyDbContext _dbContext;

        // This constructor injects an HttpClient instance and a DbContext instance.
        public CurrencyService(HttpClient httpClient, CurrencyDbContext dbContext)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
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
                await SaveRatesToDatabaseAsync(rates); // Save the rates to the database
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
        /// <returns>A list of <see cref="CurrencyRates"/> objects with rates relative to INR.</returns>
        /// <exception cref="Exception">Thrown if the INR rate is not found in the XML.</exception>
        private List<CurrencyRates> ParseXml(string xmlContent)
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
                    .Select(x => new CurrencyRates
                    {
                        CurrencyCode = x.Attribute("code").Value,
                        CurrencyDesc = x.Attribute("desc").Value,
                        Rate = Math.Round(decimal.Parse(x.Attribute("rate").Value, CultureInfo.InvariantCulture) / inrRate, 4),

                        DateTime = DateTime.UtcNow
                    }).ToList();

                // Adding DKK manually with its rate as INR per DKK
                currencyRates.Add(new CurrencyRates
                {
                    CurrencyCode = "DKK",
                    CurrencyDesc = "Danish Krone",
                    Rate = Math.Round(1 /inrRate, 4),
                    DateTime = DateTime.UtcNow
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
        private async Task SaveRatesToDatabaseAsync(List<CurrencyRates> rates)
        {
            try
            {
                _logger.Info("Updating currency rates in the database...");

                foreach (var rate in rates)
                {
                    var existingRate = await _dbContext.CurrencyRates
                        .FirstOrDefaultAsync(r => r.CurrencyCode == rate.CurrencyCode);

                    if (existingRate != null)
                    {
                        // Update existing rate
                        existingRate.Rate = rate.Rate;
                        existingRate.DateTime = rate.DateTime;
                    }
                    else
                    {
                        // Insert new rate
                        await _dbContext.CurrencyRates.AddAsync(rate);
                    }
                }

                await _dbContext.SaveChangesAsync(); // Save changes to the database
                _logger.Info("Successfully updated currency rates in the database.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error while updating currency rates in the database", ex);
                throw;
            }
        }

        /// <summary>
        /// Loads the currency exchange rates from a Database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, returning a list of currency exchange rates.</returns>
        /// <exception cref="Exception">Thrown if an error occurs while reading or deserializing the file.</exception>
        public async Task<List<CurrencyRates>> LoadRatesFromDatabaseAsync()
        {
            try
            {
                _logger.Info("Loading currency rates from database...");
                return await _dbContext.CurrencyRates.ToListAsync(); // Load rates from the database
            }
            catch (Exception ex)
            {
                _logger.Error("Error while loading currency rates from database", ex);
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
            var rates = await LoadRatesFromDatabaseAsync(); // Load exchange rates from database

            // Find the requested currency rate
            var currencyRate = rates.FirstOrDefault(r => r.CurrencyCode == currencyCode) ?? throw new ArgumentException("Invalid currency code."); // Throw exception if currency code is invalid

            return new CurrencyRateResponse
            {
                BaseCurrencyCode = currencyCode,
                BaseCurrencyAmount = 1,
                ConvertedCurrencyCode = "INR",
                ConvertedCurrencyAmount = Math.Round(currencyRate.Rate, 4)
            };
        }


        /// <summary>
        /// Converts a given amount from one currency to another based on exchange rates and save into CurrencyConversionRecord table.
        /// </summary>
        /// <param name="request">The currency conversion request containing the source and target currencies and the amount.</param>
        /// <returns>A response containing the converted amount and details of the conversion.</returns>
        /// <exception cref="ArgumentException">Thrown if the source and destination currencies are the same or invalid.</exception>
        /// <exception cref="Exception">Logs and rethrows any unexpected errors.</exception>
        public async Task<CurrencyConversionResponse> ConvertCurrencyAsync(CurrencyConversionRequest request)
        {
            try
            {
                _logger.Info("Converting currency...");

                // Check if source and destination currencies are the same
                if (request.FromCurrencyCode == request.ToCurrencyCode)
                {
                    throw new ArgumentException("FromCurrencyCode and ToCurrencyCode can't be same.");
                }

                // Load exchange rates from database
                var rates = await LoadRatesFromDatabaseAsync();

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

                // Save the conversion record
                var conversionRecord = new CurrencyConversionRecord
                {
                    FromCurrencyCode = request.FromCurrencyCode,
                    FromCurrencyDesc = fromRate.CurrencyDesc,
                    ToCurrencyCode = request.ToCurrencyCode,
                    ToCurrencyDesc = toRate.CurrencyDesc,
                    OriginalAmount = request.Amount,
                    ConvertedAmount = Math.Round(convertedAmount, 4),
                    ConversionDate = DateTime.UtcNow
                };

                await _dbContext.CurrencyConversions.AddAsync(conversionRecord); // Add the new conversion
                await _dbContext.SaveChangesAsync(); // Save changes to the database

                return new CurrencyConversionResponse
                {
                    FromCurrencyCode = request.FromCurrencyCode,
                    FromCurrencyDesc = fromRate.CurrencyDesc,
                    ToCurrencyCode = request.ToCurrencyCode,
                    ToCurrencyDesc = toRate.CurrencyDesc,
                    OriginalAmount = request.Amount,
                    ConvertedAmount = Math.Round(convertedAmount, 4)
                };
            }
            catch (Exception ex)
            {
                _logger.Error("Error while converting currency", ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the stored currency conversion records from the database.
        /// </summary>
        /// <param name="fromCurrency">The source currency code to filter by.</param>
        /// <param name="startDate">The start date to filter by.</param>
        /// <param name="endDate">The end date to filter by.</param>
        /// <returns>A list of currency conversion records matching the specified criteria.</returns>
        /// <exception cref="Exception">Thrown if an error occurs while retrieving the records.</exception>
        public async Task<List<CurrencyConversionRecord>> GetStoredConversionsAsync(string? fromCurrency = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.Info("Getting stored currency conversions...");
                var query = _dbContext.CurrencyConversions.AsQueryable();

                if (!string.IsNullOrEmpty(fromCurrency))
                {
                    query = query.Where(c => c.FromCurrencyCode == fromCurrency); // Filter by source currency
                }
                if (startDate.HasValue)
                {
                    query = query.Where(c => c.ConversionDate >= startDate.Value); // Filter by start date
                }
                if (endDate.HasValue)
                {
                    query = query.Where(c => c.ConversionDate <= endDate.Value); // Filter by end date
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Error while getting stored currency conversions", ex);
                throw;
            }
        }

    }
}
