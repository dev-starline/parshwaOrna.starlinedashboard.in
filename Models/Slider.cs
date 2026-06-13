using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class Slider
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SliderId { get; set; }

        [Required]
        public int ClientId { get; set; }

        [NotMapped]
        public IFormFile? SliderImage { get; set; }
        public string? SliderThumbnailPath { get; set; }
        public string? SliderPath { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
