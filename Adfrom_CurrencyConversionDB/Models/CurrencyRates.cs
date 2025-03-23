using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Adfrom_CurrencyConversionDB.Models
{
    public class CurrencyRates
    {
        //This class is used to store the currency rate data

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyDesc { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Rate { get; set; }
        public DateTime DateTime { get; set; }
    }
}
