using Core.Domain;
using Core.DomainServices;
using System.Collections.Generic;

namespace Portal.API.GraphQL
{
    public class ApiQuery
    {
        private readonly IDiagnoseRepository _diagnoseRepository;
        private readonly ITreatmentTypeRepository _treatmentTypeRepository;
        public ApiQuery(IDiagnoseRepository diagnoseRepository, ITreatmentTypeRepository treatmentTypeRepository)
        {
            _diagnoseRepository = diagnoseRepository;
            _treatmentTypeRepository = treatmentTypeRepository;
        }

        public IEnumerable<Diagnose> Diagnoses => _diagnoseRepository.GetAll();
        public IEnumerable<TreatmentType> TreatmentTypes => _treatmentTypeRepository.GetAll();
    }
}