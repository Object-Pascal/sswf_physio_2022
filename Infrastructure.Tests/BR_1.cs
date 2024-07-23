using Core.Domain;
using Core.DomainServices;
using Infrastructure.Tests.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Web_App.Controllers;
using Web_App.Http;
using Web_App.Models;
using Web_App.Session;
using Xunit;

namespace Infrastructure.Tests
{
    public class BR_1
    {
        private AppointmentModel _dummyAppointmentModel;
        private Appointment _dummyAppointment;
        private Appointment _dummyAppointmentTest1;
        private Appointment _dummyAppointmentTest2;
        private DummySession _dummySession;
        public BR_1()
        {
            _dummySession = new DummySession();
            _dummyAppointmentModel = new AppointmentModel()
            {
                Id = 0,
                TherapistEntry = _dummySession.Therapist.Name,
                TherapistIdEntry = _dummySession.Therapist.Id,
                Therapist = _dummySession.Therapist,
                TherapistId = _dummySession.Therapist.Id,
                PatientNumberEntry = _dummySession.Patient.PatientNumber,
                Patient = _dummySession.Patient,
                AppointmentDateTime = new DateTime(2022, 11, 8, 15, 0, 0),
                AppointmentDescription = "Mock",
                TreatmentEntry = _dummySession.Treatment.Id,
            };

            _dummyAppointment = new Appointment()
            {
                Id = 1,
                Therapist = _dummyAppointmentModel.Therapist,
                TherapistId = _dummyAppointmentModel.Therapist.Id,
                Patient = _dummyAppointmentModel.Patient,
                PatientId = _dummyAppointmentModel.Patient.Id,
                Treatment = _dummySession.Treatment,
                TreatmentId = _dummySession.Treatment.Id,
                AppointmentDateTime = _dummyAppointmentModel.AppointmentDateTime,
                AppointmentDescription = _dummyAppointmentModel.AppointmentDescription
            };

            _dummyAppointmentTest1 = new Appointment()
            {
                Id = 1,
                Therapist = _dummyAppointmentModel.Therapist,
                TherapistId = _dummyAppointmentModel.Therapist.Id,
                Patient = _dummyAppointmentModel.Patient,
                PatientId = _dummyAppointmentModel.Patient.Id,
                Treatment = _dummySession.Treatment,
                TreatmentId = _dummySession.Treatment.Id,
                AppointmentDateTime = _dummyAppointmentModel.AppointmentDateTime,
                AppointmentDescription = _dummyAppointmentModel.AppointmentDescription
            };

            _dummyAppointmentTest2 = new Appointment()
            {
                Id = 1,
                Therapist = _dummyAppointmentModel.Therapist,
                TherapistId = _dummyAppointmentModel.Therapist.Id,
                Patient = _dummyAppointmentModel.Patient,
                PatientId = _dummyAppointmentModel.Patient.Id,
                Treatment = _dummySession.Treatment,
                TreatmentId = _dummySession.Treatment.Id,
                AppointmentDateTime = _dummyAppointmentModel.AppointmentDateTime,
                AppointmentDescription = _dummyAppointmentModel.AppointmentDescription
            };
        }

        [Fact]
        public void Appointment_Add_Should_Return_View_With_No_Errors_With_No_Appointments_Made()
        {
            // Happy => No appointments have been made.

            // Arrange
            var _logger = new Mock<ILogger<AppointmentController>>();
            var _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
            var _patientRepositoryMock = new Mock<IPatientRepository>();
            var _patientFileRepositoryMock = new Mock<IPatientFileRepository>();
            var _therapistRepositoryMock = new Mock<ITherapistRepository>();
            var _treatmentRepositoryMock = new Mock<ITreatmentRepository>();
            var _treatmentPlanRepositoryMock = new Mock<ITreatmentPlanRepository>();
            var _httpContextMock = new Mock<HttpContext>();

            AppointmentController ctrl = new AppointmentController(_logger.Object, _appointmentRepositoryMock.Object, _patientRepositoryMock.Object, _patientFileRepositoryMock.Object,
                _therapistRepositoryMock.Object, _treatmentRepositoryMock.Object, _treatmentPlanRepositoryMock.Object);

            MockHttpSession mockSession = new MockHttpSession();
            mockSession.SetBoolean("IsPatient", false);
            _httpContextMock.Setup(s => s.Session).Returns(mockSession);
            ctrl.ControllerContext.HttpContext = _httpContextMock.Object;

            _patientRepositoryMock.Setup(x => x.Get(_dummySession.Patient.PatientNumber))
                .Returns(_dummySession.Patient);
            _therapistRepositoryMock.Setup(x => x.Get(_dummySession.Therapist.Id))
                .Returns(_dummySession.Therapist);
            _appointmentRepositoryMock.Setup(x => x.Add(It.IsAny<Appointment>()))
                .Returns((true, _dummyAppointment));
            _appointmentRepositoryMock.Setup(x => x.GetByPatientNumber(_dummySession.Patient.PatientNumber))
                .Returns(new List<Appointment>());
            _treatmentPlanRepositoryMock.Setup(x => x.Get(_dummySession.Patient.PatientNumber))
                .Returns(_dummySession.TreatmentPlan);

            // Act
            ActionResult result = ctrl.Add(_dummyAppointmentModel) as ActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(ctrl.ModelState.IsValid);
            Assert.Equal(0, ctrl.ModelState.ErrorCount);
        }

        [Fact]
        public void Appointment_Add_Should_Return_View_With_One_Error_When_Appointments_Exist()
        {
            // Unhappy => 2 appointments already exists with a maximum of 2 per week.

            // Arrange
            var _logger = new Mock<ILogger<AppointmentController>>();
            var _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
            var _patientRepositoryMock = new Mock<IPatientRepository>();
            var _patientFileRepositoryMock = new Mock<IPatientFileRepository>();
            var _therapistRepositoryMock = new Mock<ITherapistRepository>();
            var _treatmentRepositoryMock = new Mock<ITreatmentRepository>();
            var _treatmentPlanRepositoryMock = new Mock<ITreatmentPlanRepository>();
            var _httpContextMock = new Mock<HttpContext>();

            AppointmentController ctrl = new AppointmentController(_logger.Object, _appointmentRepositoryMock.Object, _patientRepositoryMock.Object, _patientFileRepositoryMock.Object,
                _therapistRepositoryMock.Object, _treatmentRepositoryMock.Object, _treatmentPlanRepositoryMock.Object);

            MockHttpSession mockSession = new MockHttpSession();
            mockSession.SetBoolean("IsPatient", false);
            _httpContextMock.Setup(s => s.Session).Returns(mockSession);
            ctrl.ControllerContext.HttpContext = _httpContextMock.Object;

            _patientRepositoryMock.Setup(x => x.Get(_dummySession.Patient.PatientNumber))
                .Returns(_dummySession.Patient);
            _therapistRepositoryMock.Setup(x => x.Get(_dummySession.Therapist.Id))
                .Returns(_dummySession.Therapist);
            _appointmentRepositoryMock.Setup(x => x.Add(It.IsAny<Appointment>()))
                .Returns((true, _dummyAppointment));

            _dummySession.TreatmentPlan.MaxTreatmentsPerWeek = 2;
            _dummyAppointmentTest1.AppointmentDateTime = new DateTime(2022, 11, 8, 16, 0, 0);
            _dummyAppointmentTest2.AppointmentDateTime = new DateTime(2022, 11, 8, 17, 0, 0);
            _appointmentRepositoryMock.Setup(x => x.GetByPatientNumber(_dummySession.Patient.PatientNumber))
                .Returns(new List<Appointment>()
                {
                    _dummyAppointmentTest1,
                    _dummyAppointmentTest2
                });
            _treatmentPlanRepositoryMock.Setup(x => x.Get(_dummySession.Patient.PatientNumber))
                .Returns(_dummySession.TreatmentPlan);

            // Act
            ActionResult result = ctrl.Add(_dummyAppointmentModel) as ActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(ctrl.ModelState.IsValid);
            Assert.Equal(1, ctrl.ModelState.ErrorCount);
            Assert.True(ctrl.ModelState.ContainsKey(nameof(AppointmentModel.AppointmentDateTime)));
            Assert.Equal("Maximum number of treatments reached in that week", 
                ctrl.ModelState[nameof(AppointmentModel.AppointmentDateTime)].Errors.First().ErrorMessage);
        }
    }
}
