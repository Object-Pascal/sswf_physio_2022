using Core.Domain;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories
{
    public class TreatmentPlanRepository : ITreatmentPlanRepository
    {
        private SwfContext _context;
        public TreatmentPlanRepository(SwfContext context)
        {
            _context = context;
        }

        public TreatmentPlan Get(string patientNumber)
        {
            try
            {
                Patient patient = _context.Patients.FirstOrDefault(x => x.PatientNumber == patientNumber);
                TreatmentPlan treatmentPlan = _context.TreatmentPlans.First(x => x.PatientFileId == patient.PatientFileId);
                if (treatmentPlan == null)
                {
                    PatientFile patientFile = _context.PatientFiles.FirstOrDefault(x => x.Patient.PatientNumber == patientNumber);
                    if (patientFile == null)
                        patientFile = _context.PatientFiles.First(x => _context.Patients.First(x => x.PatientNumber == patientNumber).PatientNumber == patientNumber);
                }
                return treatmentPlan;
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<Treatment> GetTreatments(int treatmentPlanId)
        {
            return _context.Treatments.Where(x => x.TreatmentPlanId == treatmentPlanId).ToList();
        }

        public (bool, Exception) AddTreatmentPlan(TreatmentPlan treatmentPlan)
        {
            try
            {
                _context.TreatmentPlans.Add(treatmentPlan);
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }

        public IEnumerable<TreatmentPlan> GetAll()
        {
            return _context.TreatmentPlans.ToList();
        }

        public (bool, Exception) Update(TreatmentPlan treatmentPlan, string patientNumber)
        {
            try
            {
                TreatmentPlan oldData = _context.PatientFiles.FirstOrDefault(x => x.Patient.PatientNumber == patientNumber).TreatmentPlan;
                if (oldData == null)
                    oldData = _context.TreatmentPlans.First(x => x.PatientFileId == _context.Patients.First(x => x.PatientNumber == patientNumber).PatientFileId);

                _context.Entry(oldData).CurrentValues.SetValues(treatmentPlan);
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }

        public (bool, Exception) Delete(string patientNumber, int treatmentPlanId)
        {
            try
            {
                _context.PatientFiles.First(x => x.Id == _context.Patients.FirstOrDefault(x => x.PatientNumber == patientNumber).PatientFileId).TreatmentPlanId = null;

                List<Treatment> treatments = _context.Treatments.Where(x => x.TreatmentPlanId == treatmentPlanId).ToList();
                treatments.ForEach(x => _context.Treatments.Remove(x));

                _context.TreatmentPlans.Remove(_context.TreatmentPlans.First(x => x.Id == treatmentPlanId));
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