using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.Domain
{
    public class Therapist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ProfilePictureBase64 { get; set; }

        [Required]
        [MaxLength(32)]
        public string Name { get; set; }

        [Required]
        public bool IsStudent { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        [MaxLength(64)]
        public string PersonNumber { get; set; }

        [MaxLength(128)]
        public string BIGNumber { get; set; }

        [Required]
        public DateTime? AvailableFrom { get; set; }

        [Required]
        public DateTime? AvailableTo { get; set; }

        public IEnumerable<TreatmentPlan> TreatmentPlans { get; set; }

        public IEnumerable<PatientFile> IntakeByPatientFiles { get; set; }

        public IEnumerable<PatientFile> UnderSuperVisionByPatientFiles { get; set; }

        public IEnumerable<PatientFile> HeadOfTreatmentPatientFiles { get; set; }

        public IEnumerable<Remark> Remarks { get; set; }

        public IEnumerable<Appointment> Appointments { get; set; }
    }
}