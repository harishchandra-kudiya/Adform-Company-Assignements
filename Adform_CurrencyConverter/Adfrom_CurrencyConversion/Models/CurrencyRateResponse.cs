namespace Adfrom_CurrencyConversion.Models
{
    public class CurrencyRateResponse
    {
        //This class is used to return the response to the currency rate request
        public string BaseCurrencyCode { get; set; }
        public decimal BaseCurrencyAmount { get; set; }
        public string ConvertedCurrencyCode { get; set; }
        public decimal ConvertedCurrencyAmount { get; set; }

    }
}
