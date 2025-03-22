using Adfrom_CurrencyConversion.Interfaces;

public class CurrencyRateUpdaterService : BackgroundService
{
    private readonly ICurrencyService _currencyService;
    private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(60);

    public CurrencyRateUpdaterService(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _currencyService.FetchAndSaveLatestRatesAsync();
            }
            catch (Exception ex)
            {
            }

            await Task.Delay(_updateInterval, stoppingToken);
        }
    }
}
