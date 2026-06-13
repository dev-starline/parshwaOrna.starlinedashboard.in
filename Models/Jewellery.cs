using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class Jewellery
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Id { get; set; }

        public int? CategoryId { get; set; } = 0;

        public int? SubCategoryId { get; set; } = 0;

        public int ClientId { get; set; }

        public bool isDisplay { get; set; }
        public string? TagNo { get; set; }

        public string? Description { get; set; }

        public string? Name { get; set; }

        public string? Image { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
