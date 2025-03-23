using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Adfrom_CurrencyConversionDB.Models
{
    public class CurrencyConversionRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string FromCurrencyCode { get; set; }
        public string FromCurrencyDesc { get; set; }
        public string ToCurrencyCode { get; set; }
        public string ToCurrencyDesc { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal OriginalAmount { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ConvertedAmount { get; set; }
        public DateTime ConversionDate { get; set; }
    }
}
