using Core.Domain;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private SwfContext _context;
        public AppointmentRepository(SwfContext context)
        {
            _context = context;
        }

        public Appointment Get(int id)
        {
            return _context.Appointments.FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<Appointment> GetAll()
        {
            return _context.Appointments.ToList();
        }

        public IEnumerable<Appointment> GetByPatientNumber(string patientNumber)
        {
            try
            {
                int patientId = _context.Patients.FirstOrDefault(x => x.PatientNumber == patientNumber).Id;
                return _context.Appointments.Where(x => x.PatientId == patientId);
            }
            catch
            {
                return new List<Appointment>();
            }
        }

        public int GetCountByPatientNumber(string patientNumber)
        {
            try
            {
                int patientId = _context.Patients.FirstOrDefault(x => x.PatientNumber == patientNumber).Id;
                return _context.Appointments.Where(x => x.PatientId == patientId).Count();
            }
            catch
            {
                return -1;
            }
        }

        public IEnumerable<Appointment> GetByTherapistId(int id)
        {
            try
            {
                return _context.Appointments.Where(x => x.TherapistId == id);
            }
            catch
            {
                return new List<Appointment>();
            }
        }

        public (bool, Appointment) Add(Appointment appointment)
        {
            try
            {
                _context.Appointments.Add(appointment);
                int entries = _context.SaveChanges();

                return entries > 0 ? (true, appointment) : (false, appointment);
            }
            catch
            {
                return (false, null);
            }
        }
        public (bool, Exception) Update(Appointment appointment)
        {
            try
            {
                Appointment oldData = _context.Appointments.First(x => x.Id == appointment.Id);
                _context.Entry(oldData).CurrentValues.SetValues(appointment);
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }

        public (bool, Exception) Delete(int id)
        {
            try
            {
                _context.Appointments.Remove(_context.Appointments.First(x => x.Id == id));
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }
    }
}