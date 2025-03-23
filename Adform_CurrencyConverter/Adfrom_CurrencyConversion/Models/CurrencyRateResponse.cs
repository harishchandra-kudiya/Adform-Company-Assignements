using System.ComponentModel.DataAnnotations.Schema;

namespace Adfrom_CurrencyConversion.Models
{
    public class CurrencyRateResponse
    {
        //This class is used to return the response to the currency rate request
        public string BaseCurrencyCode { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal BaseCurrencyAmount { get; set; }
        public string ConvertedCurrencyCode { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ConvertedCurrencyAmount { get; set; }

    }
}
