using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.Domain
{
    public class TreatmentPlan
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        public Therapist UnderTreatmentBy { get; set; }
        public int? UnderTreatmentById { get; set; }

        [Required]
        public DateTime? StartDate { get; set; }

        [Required]
        public DateTime? EndDate { get; set; }

        [Required]
        public int MaxTreatmentsPerWeek { get; set; }

        [Required]
        public PatientFile PatientFile { get; set; }
        public int? PatientFileId { get; set; }

        [Required]
        public IEnumerable<Treatment> Treatments { get; set; }
    }
}