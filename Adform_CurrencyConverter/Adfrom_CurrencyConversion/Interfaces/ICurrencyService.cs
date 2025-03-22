using Adfrom_CurrencyConversion.Models;

namespace Adfrom_CurrencyConversion.Interfaces
{
    public interface ICurrencyService
    {
        Task FetchAndSaveLatestRatesAsync();
        Task<List<CurrencyRate>> LoadRatesFromFileAsync();
        Task<CurrencyConversionResponse> ConvertCurrencyAsync(CurrencyConversionRequest request);
    }
}
