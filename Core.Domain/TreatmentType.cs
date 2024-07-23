using System.ComponentModel.DataAnnotations;

namespace Core.Domain
{
    public class TreatmentType
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Code { get; set; }

        [Required]
        [MaxLength(512)]
        public string Description { get; set; }

        [Required]
        public bool IsExplanationMandatory { get; set; }
    }
}