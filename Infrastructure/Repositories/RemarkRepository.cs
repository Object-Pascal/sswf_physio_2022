using Core.Domain;
using Core.DomainServices;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories
{
    public class RemarkRepository : IRemarkRepository
    {
        private SwfContext _context;
        public RemarkRepository(SwfContext context)
        {
            _context = context;
        }

        public bool AddRemark(Remark remark)
        {

            try
            {
                _context.Remarks.Add(remark);
                int entries = _context.SaveChanges();
                return entries > 0 ? true : false;
            
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<Remark> GetAll(string patientNumber)
        {
            try
            {
                int associatedPfId = _context.Patients.FirstOrDefault(x => x.PatientNumber == patientNumber).PatientFileId.Value;
                List<Remark> remarks = _context.Remarks.Where(x => x.PatientFileId == associatedPfId).ToList();
                remarks.ForEach(x => x.RemarkMadeBy = _context.Therapists.FirstOrDefault(t => t.Id == x.RemarkMadeById));
                remarks = remarks.OrderByDescending(x => x.PostedOn).ToList();
                return remarks;
            }
            catch
            {
                return null;
            }
        }
    }
}