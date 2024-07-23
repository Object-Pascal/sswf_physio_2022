using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Domain
{
    public class Treatment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string VektisType { get; set; }

        [MaxLength(255)]
        public string Particularities { get; set; }

        [MaxLength(128)]
        public string Room { get; set; }

        [Required]
        public DateTime? AddedDate { get; set; }

        [Required]
        public DateTime? EndDate { get; set; }

        [Required]
        public TreatmentPlan TreatmentPlan { get; set; }
        public int TreatmentPlanId { get; set; }
    }
}