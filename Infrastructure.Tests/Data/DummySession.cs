using Core.Domain;
using System;

namespace Infrastructure.Tests.Data
{
    internal class DummySession
    {
        public Therapist Therapist { get; set; }
        public Patient Patient { get; set; }
        public PatientFile PatientFile { get; set; }
        public TreatmentPlan TreatmentPlan { get; set; }
        public Treatment Treatment { get; set; }

        public DummySession()
        {
            Patient = new Patient()
            {
                Id = 0,
                ProfilePictureBase64 = "",
                IsStudent = false,
                StudentNumber = "",
                StaffNumber = "123-4",
                PatientNumber = "mock_pn",
                Name = "Mock",
                DateOfBirth = new DateTime(2000, 1, 1),
                Gender = "Male",
                Email = "mock@mock.com",
                TelephoneNumber = "1234567890",
                City = "Mock",
                Street = "Mock",
                HouseNumber = "1",

            };

            Therapist = new Therapist()
            {
                Id = 1,
                ProfilePictureBase64 = "",
                Name = "Mock",
                IsStudent = false,
                EmailAddress = "mock@mock.com",
                PersonNumber = "123",
                BIGNumber = "123",
                AvailableFrom = new DateTime(2022, 10, 28),
                AvailableTo = new DateTime(2022, 11, 10)
            };

            PatientFile = new PatientFile()
            {
                Id = 0,
                IntakeBy = Therapist,
                IntakeById = Therapist.Id,
                UnderSupervisionBy = Therapist,
                UnderSupervisionById = Therapist.Id,
                HeadOfTreatment = Therapist,
                HeadOfTreatmentId = Therapist.Id,
                TreatmentPlan = null,
                Age = 18,
                RegisterDate = new DateTime(2022, 1, 1),
                ResignDate = new DateTime(2023, 1, 1),
                GlobalDescription = "Mock",
                ExtraDescription = "Mock",
                DiagnoseCode = "1200",
                Patient = Patient,
            };

            TreatmentPlan = new TreatmentPlan()
            {
                Id = 0,
                Description = "Mock",
                UnderTreatmentBy = Therapist,
                UnderTreatmentById = Therapist.Id,
                StartDate = new DateTime(2022, 10, 24),
                EndDate = new DateTime(2022, 11, 24),
                MaxTreatmentsPerWeek = 2,
                PatientFile = PatientFile,
                PatientFileId = PatientFile.Id
            };
            PatientFile.TreatmentPlan = TreatmentPlan;

            Treatment = new Treatment()
            {
                Id = 0,
                VektisType = "1000",
                Particularities = "Mock",
                Room = "Mock",
                AddedDate = new DateTime(2022, 11, 1),
                EndDate = new DateTime(2022, 11, 8), 
                TreatmentPlan = TreatmentPlan,
                TreatmentPlanId = TreatmentPlan.Id
            };
        }
    }
}
