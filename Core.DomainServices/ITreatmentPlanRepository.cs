using Core.Domain;
using System;
using System.Collections.Generic;

namespace Core.DomainServices
{
    public interface ITreatmentPlanRepository
    {
        TreatmentPlan Get(string patientNumber);
        IEnumerable<Treatment> GetTreatments(int treatmentPlanId);
        IEnumerable<TreatmentPlan> GetAll();
        (bool, Exception) AddTreatmentPlan(TreatmentPlan treatmentPlan);
        (bool, Exception) Update(TreatmentPlan treatmentPlan, string patientNumber);
        (bool, Exception) Delete(string patientNumber, int treatmentPlanId);
    }
}