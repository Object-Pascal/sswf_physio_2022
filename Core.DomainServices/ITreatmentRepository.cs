using Core.Domain;
using System;
using System.Collections.Generic;

namespace Core.DomainServices
{
    public interface ITreatmentRepository
    {
        Treatment GetTreatment(int treatmentId);
        IEnumerable<Treatment> GetAllFromPatientFile(int patientFileId);
        (bool, Exception) AddTreatment(Treatment treatment);
        (bool, Exception) Update(Treatment treatment, int treatmentId);
        (bool, Exception) Delete(int treatmentId);
    }
}