using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Domain
{
    public class Remark
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Therapist RemarkMadeBy { get; set; }
        public int? RemarkMadeById { get; set; }

        [Required]
        public PatientFile PatientFile { get; set; }
        public int PatientFileId { get; set; }

        [Required]
        public bool IsVisibleForClient { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public DateTime? PostedOn { get; set; }
    }
}