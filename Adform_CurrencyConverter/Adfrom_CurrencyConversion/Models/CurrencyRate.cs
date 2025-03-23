using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adfrom_CurrencyConversion.Models
{
    public class CurrencyRate
    {
        //This class is used to store the currency rate data
        public string CurrencyCode { get; set; }
        public string CurrencyDesc { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Rate { get; set; }
        public DateTime DateTime { get; set; }
    }
}
