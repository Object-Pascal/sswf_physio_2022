using Core.Domain;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private SwfContext _context;
        public PatientRepository(SwfContext context)
        {
            _context = context;
        }

        public (bool, Patient) AddPatient(Patient patient)
        {
            try
            {
                _context.Patients.Add(patient);
                int entries = _context.SaveChanges();

                return entries > 0 ? (true, patient) : (false, patient);
            }
            catch
            {
                return (false, null);
            }
        }

        public (bool, Exception) Update(Patient patient)
        {
            try
            {
                Patient oldData = _context.Patients.First(x => x.PatientNumber == patient.PatientNumber);
                _context.Entry(oldData).CurrentValues.SetValues(patient);
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }

        public (bool, Exception) Delete(string patientNumber)
        {
            try
            {
                _context.Patients.Remove(_context.Patients.First(x => x.PatientNumber == patientNumber));
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }

        public Patient Get(int id)
        {
            return _context.Patients.FirstOrDefault(x => x.Id == id);
        }

        public Patient Get(string patientNumber)
        {
            return _context.Patients.FirstOrDefault(x => x.PatientNumber == patientNumber);
        }

        public Patient GetByName(string name)
        {
            try
            {
                return _context.Patients.First(x => x.Name == name);
            }
            catch
            {
                return null;
            }
        }

        public (Patient, Exception) GetByEmail(string email)
        {
            try
            {
                return (_context.Patients.First(x => x.Email == email), null);
            }
            catch (Exception e)
            {
                return (null, e);
            }
        }

        public IEnumerable<Patient> GetAll()
        {
            return _context.Patients.ToList();
        }

        public (bool, Exception) UpdateProfilePicture(int id, string base64)
        {
            try
            {
                Patient patient = _context.Patients.First(x => x.Id == id);
                patient.ProfilePictureBase64 = base64;

                _context.Patients.Update(patient);
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }

        public IEnumerable<Patient> GetByHeadOfTreatment(int therapistId)
        {
            List<PatientFile> patientFiles = _context.PatientFiles.Where(x => x.HeadOfTreatmentId.HasValue ? x.HeadOfTreatmentId.Value == therapistId : false).ToList();
            foreach (PatientFile file in patientFiles)
            {
                Patient patient = _context.Patients.First(x => x.PatientFileId == file.Id);
                if (patient != null)
                    yield return patient;
            }
        }
    }
}