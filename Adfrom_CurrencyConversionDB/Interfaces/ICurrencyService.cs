using Adfrom_CurrencyConversionDB.Models;

namespace Adfrom_CurrencyConversionDB.Interfaces
{
    public interface ICurrencyService
    {
        // This method will fetch the latest currency rates from the API and save them to a file.
        Task FetchAndSaveLatestRatesAsync();

        // This method will load the currency rates from the file.
        Task<List<CurrencyRates>> LoadRatesFromDatabaseAsync();

        // This method will return the currency rate for the given currency code.
        Task<CurrencyRateResponse> GetCurrencyRateAsync(string currencyCode);

        // This method will convert the currency based on the given request.
        Task<CurrencyConversionResponse> ConvertCurrencyAsync(CurrencyConversionRequest request);

        // This method will store the currency conversion record.
        Task<List<CurrencyConversionRecord>> GetStoredConversionsAsync(string? fromCurrency = null, DateTime? startDate = null, DateTime? endDate = null);

    }
}
