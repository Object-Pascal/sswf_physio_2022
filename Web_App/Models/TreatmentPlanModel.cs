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
    public enum TreatmentPlanState
    {
        Empty,
        Default,
        Set
    }
    public class TreatmentPlanModel
    {
        [ValidateNever]
        public TreatmentPlanState TreatmentPlanState { get; set; } = TreatmentPlanState.Empty;

        [Required]
        public int Id { get; set; }

        [DisplayName("Description")]
        [StringLength(255)]
        public string Description { get; set; }

        [Required]
        [DisplayName("Under Treatment By*")]
        public string UnderTreatmentByEntry { get; set; }
        public Therapist UnderTreatmentBy { get; set; }

        [Required]
        [DisplayName("Start Date*")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Required]
        [DisplayName("End Date*")]
        [DataType(DataType.Date)]
        [IsFutureDate]
        public DateTime? EndDate { get; set; }

        [Required]
        [DisplayName("Maximum Treatments Per Week*")]
        public int MaxTreatmentsPerWeek { get; set; }

        [ValidateNever]
        public string PatientNumber { get; set; }

        [ValidateNever]
        public IEnumerable<Treatment> Treatments { get; set; }
        [ValidateNever]
        public IEnumerable<Therapist> Therapists { get; set; }

        public static TreatmentPlanModel Create(TreatmentPlan treatmentPlan)
        {
            TreatmentPlanModel treatmentPlanModel = new TreatmentPlanModel();
            treatmentPlanModel.Id = treatmentPlan.Id;
            treatmentPlanModel.PatientNumber = treatmentPlan.PatientFile.Patient.PatientNumber;
            treatmentPlanModel.Description = treatmentPlan.Description;
            treatmentPlanModel.UnderTreatmentBy = treatmentPlan.UnderTreatmentBy;
            treatmentPlanModel.UnderTreatmentByEntry = treatmentPlanModel.UnderTreatmentBy.Name;
            treatmentPlanModel.StartDate = treatmentPlan.StartDate.HasValue ? treatmentPlan.StartDate.Value : DateTime.Now;
            treatmentPlanModel.EndDate = treatmentPlan.EndDate.HasValue ? treatmentPlan.EndDate.Value : DateTime.Now.AddDays(7);
            treatmentPlanModel.MaxTreatmentsPerWeek = treatmentPlan.MaxTreatmentsPerWeek;
            treatmentPlanModel.Treatments = treatmentPlan.Treatments;
            return treatmentPlanModel;
        }

        public static TreatmentPlanModel CreateDefault(Therapist currentTherapist, string patientNumber)
        {
            TreatmentPlanModel treatmentPlanModel = new TreatmentPlanModel();
            treatmentPlanModel.PatientNumber = patientNumber;
            treatmentPlanModel.Description = "New treatmentplan for patient: " + patientNumber;
            treatmentPlanModel.UnderTreatmentByEntry = currentTherapist.Name;
            treatmentPlanModel.StartDate = DateTime.Now;
            treatmentPlanModel.EndDate = DateTime.Now.AddDays(7);
            treatmentPlanModel.MaxTreatmentsPerWeek = 1;
            treatmentPlanModel.Treatments = new List<Treatment>();
            return treatmentPlanModel;
        }

        public TreatmentPlan Forge(ITherapistRepository therapistRepository)
        {
            TreatmentPlan treatmentPlan = new TreatmentPlan();
            treatmentPlan.Description = Description;
            treatmentPlan.UnderTreatmentBy = therapistRepository.GetByName(UnderTreatmentByEntry).Item1;
            treatmentPlan.UnderTreatmentById = treatmentPlan.UnderTreatmentBy.Id;
            treatmentPlan.StartDate = StartDate;
            treatmentPlan.EndDate = EndDate;
            treatmentPlan.MaxTreatmentsPerWeek = MaxTreatmentsPerWeek;
            treatmentPlan.Treatments = new List<Treatment>();
            return treatmentPlan;
        }
    }
}