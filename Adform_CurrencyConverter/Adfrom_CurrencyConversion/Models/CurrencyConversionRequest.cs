using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Adfrom_CurrencyConversion.Models
{
    public class CurrencyConversionRequest
    {
        // This class will be used to model the request for currency conversion.

        [Required(ErrorMessage = "FromCurrency is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "FromCurrency must be 3 characters.")]
        [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "FromCurrency must be three uppercase letters.")]
        [DefaultValue("INR")]
        public string FromCurrencyCode { get; set; } = "INR";

        [Required(ErrorMessage = "ToCurrency is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "ToCurrency must be 3 characters.")]
        [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "ToCurrency must be three uppercase letters.")]
        [DefaultValue("USD")]
        public string ToCurrencyCode { get; set; } = "USD";

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be a greater than zero.")]
        [DefaultValue(100.0000)]
        public decimal Amount { get; set; } = 100.0000m;
    }
}
