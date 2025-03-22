using Adfrom_CurrencyConversion.Interfaces;
using Adfrom_CurrencyConversion.Models;
using log4net;
using Microsoft.AspNetCore.Mvc;

namespace Adfrom_CurrencyConversion.Controllers
{
    [ApiController]
    [Route("api/currency")]
    public class CurrencyController : Controller
    {
        private readonly ICurrencyService _currencyService;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CurrencyController));

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpGet("fetch-all-latest-currency-rates")]
        public async Task<IActionResult> GetLatestRates()
        {
            try
            {
                _logger.Info("Fetching latest currency rates via API...");
                await _currencyService.FetchAndSaveLatestRatesAsync(); // Fetch latest rates and save to file
                var rates = await _currencyService.LoadRatesFromFileAsync(); // Load rates from file
                return Ok(rates);
            }
            catch (Exception ex)
            {
                _logger.Error("Error while fetching latest currency rates", ex);
                return StatusCode(500, new { message = "Internal Server Error. Please try again later." });
            }

        }

        [HttpGet("get-currency-rate/{currencyCode}")]
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

        [HttpPost("convert-currency-rate")]
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
    }
}
