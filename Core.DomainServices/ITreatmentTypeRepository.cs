using Core.Domain;
using System.Collections.Generic;

namespace Core.DomainServices
{
    public interface ITreatmentTypeRepository
    {
        IEnumerable<TreatmentType> Get(string codePart);
        IEnumerable<TreatmentType> GetAll();
        TreatmentType Delete(int id);
        TreatmentType Create(TreatmentType treatmentType);
        TreatmentType Alter(TreatmentType treatmentType);
    }
}