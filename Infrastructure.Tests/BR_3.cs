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
    public class BR_3
    {
        private AppointmentModel _dummyAppointmentModel;
        private Appointment _dummyAppointment;
        private DummySession _dummySession;
        public BR_3()
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
        }

        [Fact]
        public void Appointment_Add_Should_Return_Error_View_When_Invalid_PatientNumber()
        {
            // Unhappy => The patient doesn't exists

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

            Patient nullPatient = null;
            _dummySession.Patient.PatientNumber = "fake_value";
            _patientRepositoryMock.Setup(x => x.Get(_dummySession.Patient.PatientNumber))
                .Returns(nullPatient);
            _therapistRepositoryMock.Setup(x => x.Get(_dummySession.Therapist.Id))
                .Returns(_dummySession.Therapist);
            _appointmentRepositoryMock.Setup(x => x.Add(It.IsAny<Appointment>()))
                .Returns((true, _dummyAppointment));
            _appointmentRepositoryMock.Setup(x => x.GetByPatientNumber(_dummySession.Patient.PatientNumber))
                .Returns(new List<Appointment>());
            _treatmentPlanRepositoryMock.Setup(x => x.Get(_dummySession.Patient.PatientNumber))
                .Returns(_dummySession.TreatmentPlan);

            // Act
            IActionResult result = ctrl.Add(_dummyAppointmentModel);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
            Assert.Equal("Info", (result as RedirectToActionResult).ControllerName);
            Assert.Equal("Add Appointment - Error", (result as RedirectToActionResult).RouteValues[nameof(InfoModel.Title)]);
            Assert.Equal("The patient data could not be retrieved.", ((result as RedirectToActionResult).RouteValues[nameof(InfoModel.TableEntries)] as Dictionary<string, string>)["Reason: "]);
        }

        [Fact]
        public void Appointment_Add_Should_Return_View_With_One_Error_When_Scheduling_After_Treatment_End()
        {
            // Unhappy => Appointment gets scheduled after Treatment end

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

            // Appointment 6 days over treatment end
            _dummyAppointmentModel.AppointmentDateTime = new DateTime(2022, 11, 8, 15, 0, 0);
            _dummySession.TreatmentPlan.EndDate = new DateTime(2022, 11, 2);

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
            Assert.False(ctrl.ModelState.IsValid);
            Assert.Equal(1, ctrl.ModelState.ErrorCount);
            Assert.True(ctrl.ModelState.ContainsKey(nameof(AppointmentModel.AppointmentDateTime)));
            Assert.Equal("The Date/Time is higher than the End Date of the Treatment",
                ctrl.ModelState[nameof(AppointmentModel.AppointmentDateTime)].Errors.First().ErrorMessage);

        }

        [Fact]
        public void Appointment_Add_Should_Return_View_With_One_Error_When_Scheduling_Before_Treatment_Start()
        {
            // Unhappy => Appointment gets scheduled before Treatment start

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

            // Appointment 6 days before treatment start
            _dummyAppointmentModel.AppointmentDateTime = new DateTime(2022, 11, 1, 15, 0, 0);
            _dummySession.TreatmentPlan.StartDate = new DateTime(2022, 11, 7);

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
            Assert.False(ctrl.ModelState.IsValid);
            Assert.Equal(1, ctrl.ModelState.ErrorCount);
            Assert.True(ctrl.ModelState.ContainsKey(nameof(AppointmentModel.AppointmentDateTime)));
            Assert.Equal("The Date/Time is lower than the Start Date of the Treatment",
                ctrl.ModelState[nameof(AppointmentModel.AppointmentDateTime)].Errors.First().ErrorMessage);

        }
    }
}
