using Core.Domain;
using System;
using System.Collections.Generic;

namespace Core.DomainServices
{
    public interface IPatientFileRepository
    {
        PatientFile Get(string patientNumber);
        IEnumerable<PatientFile> GetAll();
        (bool, Exception) Update(PatientFile patientFile, string patientNumber);
        (bool, Exception) Delete(PatientFile patientFile);
        bool AddPatientFile(PatientFile patientFile);
        bool SetTreatmentPlanForFile(int patientFileId, int treatmentPlanId);
    }
}