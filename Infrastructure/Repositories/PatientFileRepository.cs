using Core.Domain;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories
{
    public class PatientFileRepository : IPatientFileRepository
    {
        private SwfContext _context;
        public PatientFileRepository(SwfContext context)
        {
            _context = context;
        }

        public bool AddPatientFile(PatientFile patientFile)
        {
            _context.PatientFiles.Add(patientFile);
            int entries = _context.SaveChanges();
            return entries > 0 ? true : false;
        }

        public (bool, Exception) Update(PatientFile patientFile, string patientNumber)
        {
            try
            {
                PatientFile oldData = _context.Patients.FirstOrDefault(x => x.PatientNumber == patientNumber).PatientFile;
                if (oldData == null)
                    oldData = _context.PatientFiles.First(x => x.Id == _context.Patients.First(x => x.PatientNumber == patientNumber).PatientFileId);

                _context.Entry(oldData).CurrentValues.SetValues(patientFile);
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }

        public (bool, Exception) Delete(PatientFile patientFile)
        {
            try
            {
                _context.PatientFiles.Remove(patientFile);
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }

        public bool SetTreatmentPlanForFile(int patientFileId, int treatmentPlanId)
        {
            try
            {
                _context.PatientFiles.First(x => x.Id == patientFileId).TreatmentPlanId = treatmentPlanId;
                int entries = _context.SaveChanges();
                return entries > 0 ? true : false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public PatientFile Get(string patientNumber)
        {
            try
            {
                PatientFile file = _context.Patients.FirstOrDefault(x => x.PatientNumber == patientNumber)?.PatientFile ?? null;
                if (file == null)
                    file = _context.PatientFiles.FirstOrDefault(x => x.Id == _context.Patients.FirstOrDefault(x => x.PatientNumber == patientNumber).PatientFileId);

                return file;
            }
            catch
            {
                return null;
            }
        }
        
        public IEnumerable<PatientFile> GetAll()
        {
            return _context.PatientFiles.ToList();
        }
    }
}