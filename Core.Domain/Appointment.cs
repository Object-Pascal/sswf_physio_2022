using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Domain
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Therapist Therapist { get; set; }
        public int? TherapistId { get; set; }

        [Required]
        public Patient Patient { get; set; }
        public int? PatientId { get; set; }

        public Treatment Treatment { get; set; }
        public int? TreatmentId { get; set; }

        [Required]
        public DateTime? AppointmentDateTime { get; set; }

        [Required]
        [MaxLength(255)]
        public string AppointmentDescription { get; set; }
    }
}