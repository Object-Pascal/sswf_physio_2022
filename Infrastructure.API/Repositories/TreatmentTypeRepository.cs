using Core.Domain;
using Core.DomainServices;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.API.Repositories
{
    public class TreatmentTypeRepository : ITreatmentTypeRepository
    {
        private SwfApiContext _context;
        public TreatmentTypeRepository(SwfApiContext context)
        {
            _context = context;
        }

        public TreatmentType Alter(TreatmentType treatmentType)
        {
            try
            {
                TreatmentType oldData = _context.TreatmentTypes.FirstOrDefault(x => x.Id == treatmentType.Id);
                _context.Entry(oldData).CurrentValues.SetValues(treatmentType);
                _context.SaveChanges();
                return oldData;
            }
            catch
            {
                return null;
            }
        }

        public TreatmentType Create(TreatmentType treatmentType)
        {
            try
            {
                TreatmentType newEntity = _context.TreatmentTypes.Add(treatmentType).Entity;
                _context.SaveChanges();
                return newEntity;
            }
            catch
            {
                return null;
            }
        }

        public TreatmentType Delete(int id)
        {
            try
            {
                TreatmentType oldEntity = _context.TreatmentTypes.Remove(_context.TreatmentTypes.FirstOrDefault(x => x.Id == id)).Entity;
                _context.SaveChanges();
                return oldEntity;
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<TreatmentType> Get(string codePart)
        {
            try
            {
                return _context.TreatmentTypes.Where(x => x.Code == codePart || x.Code.StartsWith(codePart));
            }
            catch
            {
                return null;
            }

        }

        public IEnumerable<TreatmentType> GetAll()
        {
            return _context.TreatmentTypes.ToList();
        }
    }
}