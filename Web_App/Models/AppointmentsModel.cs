using Core.Domain;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Web_App.Models
{
    public class AppointmentsModel
    {
        public List<Appointment> Appointments { get; set; }
        public AppointmentsModel(List<Appointment> appointments)
        {
            Appointments = appointments;
        }

        public static AppointmentsModel Create(List<Appointment> appointments, ITherapistRepository therapistRepository, IPatientRepository patientRepository)
        {
            List<Appointment> list = appointments.Where(x => x.AppointmentDateTime.Value.Year == DateTime.Now.Year && x.AppointmentDateTime.Value.DayOfYear == DateTime.Now.DayOfYear).ToList();
            list = list.Select(x =>
            {
                x.Therapist = x.Therapist ?? therapistRepository.Get(x.TherapistId.Value);
                x.Patient = x.Patient ?? patientRepository.Get(x.PatientId.Value);
                return x;
            }).OrderByDescending(x => x.AppointmentDateTime).ToList();

            List<Appointment> buff = appointments.Select(x =>
            {
                x.Therapist = x.Therapist ?? therapistRepository.Get(x.TherapistId.Value);
                x.Patient = x.Patient ?? patientRepository.Get(x.PatientId.Value);
                return list.Any(a => a.Id == x.Id) ? null : x;
            }).Where(x => x != null).OrderByDescending(x => x.AppointmentDateTime).ToList();
            list.AddRange(buff);
            return new AppointmentsModel(list);
        }
    }
}