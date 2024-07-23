using Core.Domain;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories
{
    public class TreatmentRepository : ITreatmentRepository
    {
        private SwfContext _context;
        public TreatmentRepository(SwfContext context)
        {
            _context = context;
        }

        public Treatment GetTreatment(int treatmentId)
        {
            try
            {
                return _context.Treatments.FirstOrDefault(x => x.Id == treatmentId);
            }
            catch
            {
                return null;
            }
        }

        public (bool, Exception) AddTreatment(Treatment treatment)
        {
            try
            {
                _context.Treatments.Add(treatment);
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }

        public (bool, Exception) Update(Treatment treatment, int treatmentId)
        {
            try
            {
                Treatment oldData = _context.Treatments.FirstOrDefault(x => x.Id == treatmentId);
                _context.Entry(oldData).CurrentValues.SetValues(treatment);
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }

        public (bool, Exception) Delete(int treatmentId)
        {
            try
            {
                _context.Treatments.Remove(_context.Treatments.FirstOrDefault(x => x.Id == treatmentId));
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }

        public IEnumerable<Treatment> GetAllFromPatientFile(int patientFileId)
        {
            TreatmentPlan treatmentPlan = _context.TreatmentPlans.FirstOrDefault(x => x.PatientFileId.HasValue ? x.PatientFileId.Value == patientFileId : false);
            if (treatmentPlan != null)
                return _context.Treatments.Where(x => x.TreatmentPlanId == treatmentPlan.Id);

            return null;
        }
    }
}