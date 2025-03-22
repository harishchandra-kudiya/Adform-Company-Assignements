using Adfrom_CurrencyConversion.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Adfrom_CurrencyConversion.Models
{
    public class CurrencyConversionResponse
    {
        public string FromCurrencyCode { get; set; }

        public string? FromCurrencyDesc { get; set; }

        public string ToCurrencyCode { get; set; }

        public string? ToCurrencyDesc { get; set; }

        public decimal OriginalAmount { get; set; }
        public decimal ConvertedAmount { get; set; }

    }
}
