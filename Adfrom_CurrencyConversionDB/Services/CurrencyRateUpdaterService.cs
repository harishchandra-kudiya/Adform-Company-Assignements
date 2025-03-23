using Adfrom_CurrencyConversionDB.Data;
using Adfrom_CurrencyConversionDB.Interfaces;
using log4net;

namespace Adfrom_CurrencyConversionDB.Services
{
    public class CurrencyRateUpdaterService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory; // Use scope for DbContext
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CurrencyRateUpdaterService));
        private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(60);



        /// <summary>
        /// Initializes a new instance of <see cref="CurrencyRateUpdaterService"/>.
        /// </summary>
        /// <param name="currencyService">Service for fetching currency rates.</param>
        /// <param name="serviceScopeFactory">Factory to create service scope for DbContext.</param>
        public CurrencyRateUpdaterService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
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
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var currencyService = scope.ServiceProvider.GetRequiredService<ICurrencyService>();
                        await currencyService.FetchAndSaveLatestRatesAsync();
                    }
                    _logger.Info("Successfully updated currency exchange rates in the database.");
                }
                catch (Exception ex)
                {
                    _logger.Error("Error occurred while updating currency exchange rates.", ex);
                }

                _logger.Info($"Waiting for {_updateInterval.TotalMinutes} minutes before the next update.");
                await Task.Delay(_updateInterval, stoppingToken);
            }

            _logger.Info("Currency Rate Updater Service is stopping.");
        }

    }
}
