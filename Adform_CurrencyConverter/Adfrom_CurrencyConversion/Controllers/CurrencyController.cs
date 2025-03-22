using Adfrom_CurrencyConversion.Interfaces;
using Adfrom_CurrencyConversion.Models;
using Microsoft.AspNetCore.Mvc;

namespace Adfrom_CurrencyConversion.Controllers
{
    [ApiController]
    [Route("api/currency")]
    public class CurrencyController : Controller
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpGet("fetch-all-latest-currency-rates")]
        public async Task<IActionResult> GetLatestRates()
        {
            await _currencyService.FetchAndSaveLatestRatesAsync(); // Fetch new data
            var rates = await _currencyService.LoadRatesFromFileAsync();
            return Ok(rates);
        }

        [HttpPost("convert")]
        public async Task<IActionResult> ConvertCurrency([FromBody] CurrencyConversionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _currencyService.ConvertCurrencyAsync(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
