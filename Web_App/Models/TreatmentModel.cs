using Core.Domain;
using Core.DomainServices;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Web_App.Validation;

namespace Web_App.Models
{
    public class TreatmentModel
    {
        [Required]
        [DisplayName("Treatment Id*")]
        public int Id { get; set; }

        [Required]
        [DisplayName("Vektis Type*")]
        [StringLength(255)]
        public string VektisType { get; set; }

        [StringLength(255)]
        [DisplayName("Particularities")]
        public string Particularities { get; set; }

        [StringLength(128)]
        [DisplayName("Room")]
        public string Room { get; set; }

        [Required]
        [DisplayName("Added Date*")]
        [DataType(DataType.Date)]
        public DateTime? AddedDate { get; set; }

        [Required]
        [DisplayName("End Date*")]
        [DataType(DataType.Date)]
        [IsFutureDate]
        public DateTime? EndDate { get; set; }

        [ValidateNever]
        public IEnumerable<TreatmentType> TreatmentTypes { get; set; }

        [ValidateNever]
        [DisplayName("Treatment Plan Id*")]
        public int TreatmentPlanId { get; set; }

        [ValidateNever]
        [DisplayName("Patient Number*")]
        public string PatientNumber { get; set; }

        [ValidateNever]
        public string PatientName { get; set; }

        public static TreatmentModel Create(Treatment treatment, string patientNumber, string patientName)
        {
            TreatmentModel treatmentModel = new TreatmentModel();
            treatmentModel.PatientNumber = patientNumber;
            treatmentModel.PatientName = patientName;
            treatmentModel.Id = treatment.Id;
            treatmentModel.TreatmentPlanId = treatment.TreatmentPlanId;
            treatmentModel.VektisType = treatment.VektisType;
            treatmentModel.Particularities = treatment.Particularities;
            treatmentModel.Room = treatment.Room;
            treatmentModel.AddedDate = treatment.AddedDate;
            treatmentModel.EndDate = treatment.EndDate;
            return treatmentModel;
        }

        public Treatment Forge()
        {
            Treatment treatment = new Treatment();
            treatment.TreatmentPlanId = TreatmentPlanId;
            treatment.VektisType = VektisType;
            treatment.Particularities = Particularities;
            treatment.Room = Room;
            treatment.AddedDate = AddedDate;
            treatment.EndDate = EndDate;
            return treatment;
        }
    }
}