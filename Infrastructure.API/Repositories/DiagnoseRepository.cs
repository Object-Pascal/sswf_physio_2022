using Core.Domain;
using Core.DomainServices;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.API.Repositories
{
    public class DiagnoseRepository : IDiagnoseRepository
    {
        private SwfApiContext _context;
        public DiagnoseRepository(SwfApiContext context)
        {
            _context = context;
        }

        public Diagnose Alter(Diagnose diagnose)
        {
            try
            {
                Diagnose oldData = _context.Diagnoses.FirstOrDefault(x => x.Id == diagnose.Id);
                _context.Entry(oldData).CurrentValues.SetValues(diagnose);
                _context.SaveChanges();
                return oldData;
            }
            catch
            {
                return null;
            }
        }

        public Diagnose Create(Diagnose diagnose)
        {
            try
            {
                Diagnose newEntity = _context.Diagnoses.Add(diagnose).Entity;
                _context.SaveChanges();
                return newEntity;
            }
            catch
            {
                return null;
            }
        }

        public Diagnose Delete(int id)
        {
            try
            {
                Diagnose oldEntity = _context.Diagnoses.Remove(_context.Diagnoses.FirstOrDefault(x => x.Id == id)).Entity;
                _context.SaveChanges();
                return oldEntity;
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<Diagnose> Get(string codePart)
        {
            try
            {
                return _context.Diagnoses.Where(x => x.Code == codePart || x.Code.StartsWith(codePart));
            }
            catch
            {
                return null;
            } 
            
        }

        public IEnumerable<Diagnose> GetAll()
        {
            return _context.Diagnoses.ToList();
        }
    }
}