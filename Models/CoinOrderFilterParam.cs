using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class CoinOrderFilterParam
    {
        [Required(ErrorMessage = "FromDate is required.")]
        [RegularExpression(@"(((0|1)[0-9]|2[0-9]|3[0-1])\/(0[1-9]|1[0-2])\/((19|20)\d\d))$", ErrorMessage = "Invalid date format.")]
        public string FromDate { get; set; }

        [Required(ErrorMessage = "ToDate is required.")]
        [RegularExpression(@"(((0|1)[0-9]|2[0-9]|3[0-1])\/(0[1-9]|1[0-2])\/((19|20)\d\d))$", ErrorMessage = "Invalid date format.")]
        public string ToDate { get; set; }

    }
}
