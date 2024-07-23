using Core.Domain;
using System;
using System.Collections.Generic;

namespace Core.DomainServices
{
    public interface IPatientRepository
    {
        Patient Get(int id);
        Patient Get(string patientNumber);
        Patient GetByName(string name);
        (Patient, Exception) GetByEmail(string email);
        IEnumerable<Patient> GetAll();
        IEnumerable<Patient> GetByHeadOfTreatment(int therapistId);
        (bool, Exception) Update(Patient patient);
        (bool, Exception) UpdateProfilePicture(int id, string base64);
        (bool, Exception) Delete(string patientNumber);
        (bool r, Patient p) AddPatient(Patient patient);
    }
}