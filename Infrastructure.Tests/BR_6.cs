using Core.Domain;
using Core.DomainServices;
using Infrastructure.Tests.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Web_App.Controllers;
using Web_App.Http;
using Web_App.Models;
using Web_App.Session;
using Xunit;

namespace Infrastructure.Tests
{
    public class BR_6
    {
        private AppointmentModel _dummyAppointmentModel;
        private Appointment _dummyAppointment;
        private DummySession _dummySession;
        public BR_6()
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
        public void Appointment_Cancel_Should_Return_View_With_One_Error_When_Appointment_Is_Cancelled_In_Less_Than_24_Hours()
        {
            // Unhappy => Appointments can not be cancelled in less than 24 hours

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
            mockSession.SetBoolean("IsPatient", true);
            mockSession["UserDataId"] = _dummyAppointmentModel.Patient.Id;
            _httpContextMock.Setup(s => s.Session).Returns(mockSession);
            ctrl.ControllerContext.HttpContext = _httpContextMock.Object;

            _dummyAppointment.AppointmentDateTime = DateTime.Now.AddHours(20);
            _appointmentRepositoryMock.Setup(x => x.Get(_dummyAppointmentModel.Id))
                .Returns(_dummyAppointment);
            _patientRepositoryMock.Setup(x => x.Get(_dummyAppointmentModel.Patient.Id))
                .Returns(_dummySession.Patient);
            _appointmentRepositoryMock.Setup(x => x.Delete(_dummyAppointmentModel.Id))
                .Returns((true, null));

            // Act
            ActionResult result = ctrl.Cancel(_dummyAppointmentModel.Id.ToString()) as ActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
            Assert.Equal("Info", (result as RedirectToActionResult).ControllerName);
            Assert.Equal("Cancel Appointment - Error", (result as RedirectToActionResult).RouteValues[nameof(InfoModel.Title)]);
            Assert.Equal("You are not allowed to cancel an appointment coming up within 24 hours", ((result as RedirectToActionResult).RouteValues[nameof(InfoModel.TableEntries)] as Dictionary<string, string>)["Reason: "]);
        }
    }
}
