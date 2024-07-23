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
    public class AppointmentModel
    {
        [ValidateNever]
        [DisplayName("Appointment Id*")]
        public int Id { get; set; }

        public string TherapistEntry { get; set; }

        [Required]
        [DisplayName("Therapist/Student")]
        public int TherapistIdEntry { get; set; }

        [ValidateNever]
        public Therapist Therapist { get; set; }

        [DisplayName("Patient Name")]
        public string PatientNameEntry { get; set; }

        [Required]
        [DisplayName("Patient Number")]
        public string PatientNumberEntry { get; set; }

        [ValidateNever]
        public Patient Patient { get; set; }

        [Required]
        [DisplayName("Appointment Date/Time*")]
        [DataType(DataType.DateTime)]
        [IsFutureDate]
        public DateTime? AppointmentDateTime { get; set; }

        [Required]
        [DisplayName("Description*")]
        [MaxLength(255)]
        public string AppointmentDescription { get; set; }

        public IEnumerable<Therapist> Therapists { get; set; }
        public IEnumerable<Treatment> Treatments { get; set; }

        [Required]
        [DisplayName("Treatment*")]
        public int TreatmentEntry { get; set; }
        public int TherapistId { get; set; }

        public IEnumerable<Patient> Patients { get; set; }


        public Appointment ForgeAppointment(ITherapistRepository therapistRepository, IPatientRepository patientRepository, ITreatmentRepository treatmentRepository)
        {
            Appointment appointmentDetails = new Appointment();
            appointmentDetails.Id = Id;

            appointmentDetails.Therapist = therapistRepository.Get(TherapistId);
            appointmentDetails.TherapistId = appointmentDetails.Therapist.Id;

            appointmentDetails.Patient = patientRepository.Get(PatientNumberEntry);
            appointmentDetails.PatientId = appointmentDetails.Patient.Id;

            if (TreatmentEntry > 0)
            {
                appointmentDetails.Treatment = treatmentRepository.GetTreatment(TreatmentEntry);
                appointmentDetails.TreatmentId = appointmentDetails.Treatment.Id;
            }

            appointmentDetails.AppointmentDateTime = AppointmentDateTime;
            appointmentDetails.AppointmentDescription = AppointmentDescription;
            return appointmentDetails;
        }

        public static AppointmentModel Create(Appointment appointment, int patientFileId, ITherapistRepository therapistRepository, IPatientRepository patientRepository, ITreatmentRepository treatmentRepository)
        {
            AppointmentModel appointmentModel = new AppointmentModel();
            appointmentModel.Id = appointment.Id;

            appointmentModel.Therapist = appointment.Therapist ?? therapistRepository.Get(appointment.TherapistId.Value);
            appointmentModel.TherapistEntry = appointmentModel.Therapist.Name;
            appointmentModel.TherapistId = appointmentModel.Therapist.Id;

            appointmentModel.Patient = appointment.Patient ?? patientRepository.Get(appointment.PatientId.Value);
            appointmentModel.PatientNameEntry = appointmentModel.Patient.Name;
            appointmentModel.PatientNumberEntry = appointmentModel.Patient.PatientNumber;

            appointmentModel.Treatments = treatmentRepository.GetAllFromPatientFile(patientFileId) ?? new List<Treatment>();
            appointmentModel.TreatmentEntry = appointment.TreatmentId.HasValue ? appointment.TreatmentId.Value : 0;

            appointmentModel.AppointmentDateTime = appointment.AppointmentDateTime;
            appointmentModel.AppointmentDescription = appointment.AppointmentDescription;
            return appointmentModel;
        }
    }
}