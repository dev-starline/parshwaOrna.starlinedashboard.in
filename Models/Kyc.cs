using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class Kyc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        public int clientId { get; set; }
        [Required]
        public string mobile { get; set; }
        public string? companyName { get; set; }
        public string? companyAddress { get; set; }
        public string? name { get; set; }
        public string? partnerName { get; set; }
        public string? partnerMobile { get; set; }
        public string? officeMobile1 { get; set; }
        public string? officeMobile2 { get; set; }
        public string? residenceAddress { get; set; }
        public string? mail { get; set; }
        public string? bankName { get; set; }
        public string? branchName { get; set; }
        public string? accountNumber { get; set; }
        public string? ifsc { get; set; }
        public string? gstNumber { get; set; }
        public string? panNumber { get; set; }
        public string? reference { get; set; }
        public string? url { get; set; }
        [NotMapped]
        public List<IFormFile>? Files { get; set; }
        [NotMapped]
        [Required]
        public string user { get; set; }
        public DateTime modifiedDate { get; set; } = DateTime.Now;
    }
}
