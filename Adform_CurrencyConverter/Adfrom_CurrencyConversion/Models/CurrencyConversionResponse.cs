using Adfrom_CurrencyConversion.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adfrom_CurrencyConversion.Models
{
    public class CurrencyConversionResponse
    {
        //This class is used to return the response to the currency conversion request

        public string FromCurrencyCode { get; set; }

        public string? FromCurrencyDesc { get; set; }

        public string ToCurrencyCode { get; set; }

        public string? ToCurrencyDesc { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal OriginalAmount { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ConvertedAmount { get; set; }

    }
}
