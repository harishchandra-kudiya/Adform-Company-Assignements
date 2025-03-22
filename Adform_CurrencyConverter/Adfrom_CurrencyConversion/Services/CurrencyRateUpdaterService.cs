using Adfrom_CurrencyConversion.Interfaces;
using Adfrom_CurrencyConversion.Services;
using log4net;

public class CurrencyRateUpdaterService : BackgroundService
{
    private readonly ICurrencyService _currencyService;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(CurrencyRateUpdaterService));
    private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(60);

    /// <summary>
    /// Initializes a new instance of <see cref="CurrencyRateUpdaterService"/>.
    /// </summary>
    /// <param name="currencyService">Service for fetching currency rates.</param>
    /// <param name="logger">Logger instance for logging events.</param>
    public CurrencyRateUpdaterService(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    /// <summary>
    /// Executes the background task to update currency rates at a fixed interval.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token to stop the background service.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Info("Currency Rate Updater Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.Info("Fetching latest currency exchange rates...");
                await _currencyService.FetchAndSaveLatestRatesAsync();
                _logger.Info("Successfully updated currency exchange rates.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error occurred while fetching currency exchange rates.", ex);
            }

            _logger.Info($"Waiting for {_updateInterval.TotalMinutes} minutes before the next update.");
            await Task.Delay(_updateInterval, stoppingToken);
        }

        _logger.Info("Currency Rate Updater Service is stopping.");
    }
}
