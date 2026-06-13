using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class Bank
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        public int clientId { get; set; }
        public string accountName { get; set; }
        public string bankName { get; set; }
        public string accountNumber { get; set; }
        public string ifscCode { get; set; }
        public string branchName { get; set; }
        public string? bankLogo { get; set; }
        public string? bankLogoUrl { get; set; }
        public DateTime modifiedDate { get; set; } = DateTime.Now;
    }
}
