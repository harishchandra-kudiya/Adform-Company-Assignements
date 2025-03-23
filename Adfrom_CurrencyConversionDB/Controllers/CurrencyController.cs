using Adfrom_CurrencyConversionDB.Interfaces;
using Adfrom_CurrencyConversionDB.Models;
using log4net;
using Microsoft.AspNetCore.Mvc;

namespace Adfrom_CurrencyConversionDB.Controllers
{
    [ApiController]
    [Route("api/currency")]
    [Produces("application/json")]
    public class CurrencyController : Controller
    {
        private readonly ICurrencyService _currencyService;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CurrencyController));

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        /// <summary>
        /// Fetches the latest currency rates from the external API and stores them.
        /// </summary>
        /// <returns>Returns the latest currency rates.</returns>
        [HttpGet("fetch-all-latest-currency-rates")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLatestRates()
        {
            try
            {
                _logger.Info("Fetching latest currency rates via API...");
                await _currencyService.FetchAndSaveLatestRatesAsync(); // Fetch latest rates and save to file
                var rates = await _currencyService.LoadRatesFromDatabaseAsync(); // Load rates from file

                if (rates == null || !rates.Any())
                {
                    _logger.Warn("No currency rates found.");
                    return NotFound(new { message = "No currency rates available." });
                }
                return Ok(rates);
            }
            catch (Exception ex)
            {
                _logger.Error("Error while fetching latest currency rates", ex);
                return StatusCode(500, new { message = "Internal Server Error. Please try again later." });
            }

        }

        /// <summary>
        /// Retrieves the exchange rate for a specific currency.
        /// </summary>
        /// <param name="currencyCode">Currency code (e.g., "USD").</param>
        /// <returns>The exchange rate for the specified currency.</returns>
        [HttpGet("get-currency-rate/{currencyCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrencyRate(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                _logger.Warn("Currency code is missing in request.");
                return BadRequest(new { message = "Currency code is required." });
            }

            try
            {
                _logger.Info($"Fetching rate for currency: {currencyCode}");
                var response = await _currencyService.GetCurrencyRateAsync(currencyCode); // Fetch rate for given currency code

                if (response == null)
                {
                    _logger.Warn($"Currency rate not found for {currencyCode}.");
                    return NotFound(new { message = $"Rate not found for currency code: {currencyCode}" });
                }
                _logger.Info($"Retrieved rate: 1 INR = {response.ConvertedCurrencyAmount} {currencyCode}");

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn($"Invalid currency code: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while fetching currency rate for {currencyCode}", ex);
                return StatusCode(500, new { message = "Internal Server Error. Please try again later." });
            }
        }

        /// <summary>
        /// Converts an amount from one currency to another.
        /// </summary>
        /// <param name="request">Conversion request object.</param>
        /// <returns>The converted amount and details.</returns>
        [HttpPost("convert-currency-rate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConvertCurrency([FromBody] CurrencyConversionRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.Warn("Invalid currency conversion request.");
                return BadRequest(ModelState);
            }

            try
            {
                _logger.Info($"Converting {request.Amount} {request.FromCurrencyCode} to {request.ToCurrencyCode}.");
                var response = await _currencyService.ConvertCurrencyAsync(request);
                if (response == null)
                {
                    _logger.Warn("Currency conversion failed due to invalid input.");
                    return BadRequest(new { message = "Conversion failed. Please check the currency codes and amount." });
                }

                _logger.Info($"Conversion successful: {response.OriginalAmount} {response.FromCurrencyCode} = {response.ConvertedAmount} {response.ToCurrencyCode}");
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn($"Invalid currency conversion request: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error during currency conversion from {request.FromCurrencyCode} to {request.ToCurrencyCode}", ex);
                return StatusCode(500, new { message = "Internal Server Error. Please try again later." });
            }
        }

        /// <summary>
        /// Retrieves stored currency conversion records filtered by currency and date range.
        /// </summary>
        /// <param name="fromCurrency">Currency code to filter (optional).</param>
        /// <param name="startDate">Start date for filtering (optional).</param>
        /// <param name="endDate">End date for filtering (optional).</param>
        /// <returns>List of stored conversions.</returns>
        [HttpGet("get-stored-conversions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
         public async Task<IActionResult> GetStoredConversions([FromQuery] string? fromCurrency, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
         {
            if (startDate > endDate)
            {
                _logger.Warn("Invalid date range provided.");
                return BadRequest(new { message = "Start date cannot be later than end date." });
            }
            try
            {
                _logger.Info($"Getting Converted Currency Data for {fromCurrency} from {startDate} to {endDate}.");
                var conversions = await _currencyService.GetStoredConversionsAsync(fromCurrency, startDate, endDate);

                if (conversions == null || !conversions.Any())
                {
                    _logger.Warn("No conversions found for the given filters.");
                    return NotFound(new { message = "No stored conversions found for the given filters." });
                }

                return Ok(conversions);
            }
            catch (Exception ex)
            {
                _logger.Error("Error while retrieving stored conversions", ex);
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }

    }
}
