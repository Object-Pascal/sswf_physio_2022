using Core.Domain;
using System;
using System.Collections.Generic;

namespace Core.DomainServices
{
    public interface IAppointmentRepository
    {
        Appointment Get(int id);
        IEnumerable<Appointment> GetAll();
        IEnumerable<Appointment> GetByPatientNumber(string patientNumber);
        int GetCountByPatientNumber(string patientNumber);
        IEnumerable<Appointment> GetByTherapistId(int id);
        (bool, Appointment) Add(Appointment appointment);
        (bool, Exception) Update(Appointment appointment);
        (bool, Exception) Delete(int id);
    }
}