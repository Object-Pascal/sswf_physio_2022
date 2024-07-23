using Core.Domain;
using System.Collections.Generic;

namespace Web_App.Models
{
    public class PatientsModel
    {
        public IEnumerable<Patient> Patients { get; set; }

        public PatientsModel(IEnumerable<Patient> patients)
        {
            Patients = patients;
        }
    }
}