using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.Domain
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ProfilePictureBase64 { get; set; }

        [Required]
        public bool IsStudent { get; set; }

        [MaxLength(32)]
        public string StudentNumber { get; set; }

        [MaxLength(32)]
        public string StaffNumber { get; set; }

        [Required]
        public string PatientNumber { get; set; }

        [Required]
        [MaxLength(32)]
        public string Name { get; set; }

        [Required]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        [MaxLength(16)]
        public string Gender { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(64)]
        public string Email { get; set; }

        [Required]
        [MaxLength(32)]
        public string TelephoneNumber { get; set; }

        [Required]
        [MaxLength(64)]
        public string City { get; set; }

        [Required]
        [MaxLength(64)]
        public string Street { get; set; }

        [Required]
        [MaxLength(12)]
        public string HouseNumber { get; set; }

        [Required]
        public PatientFile PatientFile { get; set; }
        public int? PatientFileId { get; set; }

        public IEnumerable<Appointment> Appointments { get; set; }
    }
}