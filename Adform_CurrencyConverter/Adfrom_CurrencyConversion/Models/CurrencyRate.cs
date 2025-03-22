using System.ComponentModel.DataAnnotations;

namespace Adfrom_CurrencyConversion.Models
{
    public class CurrencyRate
    {
        //This class is used to store the currency rate data
        public string CurrencyCode { get; set; }
        public string CurrencyDesc { get; set; }
        public decimal Rate { get; set; }
        public string DateTime { get; set; }
    }
}
