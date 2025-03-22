using System.ComponentModel.DataAnnotations;

namespace Adfrom_CurrencyConversion.Models
{
    public class CurrencyRate
    {
        public string CurrencyCode { get; set; }
        public string CurrencyDesc { get; set; }
        public decimal Rate { get; set; }
        public DateTime DateTime { get; set; }
    }
}
