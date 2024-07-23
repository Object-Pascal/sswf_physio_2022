using Core.Domain;
using System.Collections.Generic;

namespace Core.DomainServices
{
    public interface IDiagnoseRepository
    {
        IEnumerable<Diagnose> Get(string codePart);
        IEnumerable<Diagnose> GetAll();
        Diagnose Delete(int id);
        Diagnose Create(Diagnose diagnose);
        Diagnose Alter(Diagnose diagnose);
    }
}