using System.ComponentModel.DataAnnotations;

namespace Core.Domain
{
    public class Diagnose
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        [MaxLength(8)]
        public string Code { get; set; }

        [Required]
        [MaxLength(255)]
        public string BodyLocalization { get; set; }

        [Required]
        [MaxLength(512)]
        public string Pathology { get; set; }
    }
}