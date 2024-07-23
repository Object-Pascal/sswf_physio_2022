using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.Domain
{
    public class PatientFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Therapist IntakeBy { get; set; }
        public int? IntakeById { get; set; }

        public Therapist UnderSupervisionBy { get; set; }
        public int? UnderSupervisionById { get; set; }

        [Required]
        public Therapist HeadOfTreatment { get; set; }
        public int? HeadOfTreatmentId { get; set; }

        public TreatmentPlan TreatmentPlan { get; set; }
        public int? TreatmentPlanId { get; set; }

        [Required]
        public int Age { get; set; }

        [Required]
        public DateTime? RegisterDate { get; set; }

        public DateTime? ResignDate { get; set; }

        [Required]
        [MaxLength(255)]
        public string GlobalDescription { get; set; }

        [MaxLength(255)]
        public string ExtraDescription { get; set; }

        [Required]
        [MaxLength(255)]
        public string DiagnoseCode { get; set; }

        public Patient Patient { get; set; }

        public IEnumerable<Remark> Remarks { get; set; }
    }
}