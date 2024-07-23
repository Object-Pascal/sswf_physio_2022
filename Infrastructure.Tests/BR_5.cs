using Core.Domain;
using Core.DomainServices;
using Infrastructure.Tests.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Web_App.Controllers;
using Web_App.Http;
using Web_App.Models;
using Web_App.Session;
using Web_App.Validation;
using Xunit;

namespace Infrastructure.Tests
{
    public class BR_5
    {
        private DummySession _dummySession;
        private PatientModel _dummyPatientModel;
        public BR_5()
        {
            _dummySession = new DummySession();
            _dummyPatientModel = new PatientModel()
            {
                PatientNumber = _dummySession.Patient.PatientNumber,
                Name = _dummySession.Patient.Name,
                IsStudent = _dummySession.Patient.IsStudent,
                StudentNumber = _dummySession.Patient.StudentNumber,
                StaffNumber = _dummySession.Patient.StaffNumber,
                DateOfBirth = _dummySession.Patient.DateOfBirth,
                Gender = _dummySession.Patient.Gender,
                Email = _dummySession.Patient.Email,
                TelephoneNumber = _dummySession.Patient.TelephoneNumber,
                City = _dummySession.Patient.City,
                Street = _dummySession.Patient.Street,
                HouseNumber = _dummySession.Patient.HouseNumber,
                IntakeByEntry = _dummySession.PatientFile.IntakeBy.Name,
                UnderSupervisionByEntry = _dummySession.PatientFile.UnderSupervisionBy.Name,
                HeadOfTreatmentEntry = _dummySession.PatientFile.HeadOfTreatment.Name,
                Age = null,
                RegisterDate = _dummySession.PatientFile.RegisterDate,
                HasResignDate = true,
                ResignDate = _dummySession.PatientFile.ResignDate,
                GlobalDescription = _dummySession.PatientFile.GlobalDescription,
                ExtraDescription = _dummySession.PatientFile.ExtraDescription,
                DiagnoseCode = _dummySession.PatientFile.DiagnoseCode,
                VektisDiagnoses = new List<Diagnose>(),
                Therapists = new List<Therapist>() 
                {
                    _dummySession.Therapist
                }
            };
        }

        private Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> ls) where TUser : class
        {
            Mock<IUserStore<TUser>> store = new Mock<IUserStore<TUser>>();
            Mock<UserManager<TUser>> mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<TUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

            mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<TUser, string>((x, y) => ls.Add(x));
            mgr.Setup(x => x.AddToRoleAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<TUser, string>((x, y) => ls.Add(x));
            return mgr;
        }

        [Fact]
        public void Patient_Add_Should_Return_View_With_No_Errors_When_Age_Over_16()
        {
            // Happy => Patient is over the age of 16

            // Arrange
            var _logger = new Mock<ILogger<PatientController>>();
            var _userManagerMock = MockUserManager(new List<IdentityUser>
            {
                new IdentityUser() { Id = "1", UserName = _dummySession.Patient.Name, Email = _dummySession.Patient.Email}
            });
            var _remarkRepositoryMock = new Mock<IRemarkRepository>();
            var _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
            var _patientRepositoryMock = new Mock<IPatientRepository>();
            var _patientFileRepositoryMock = new Mock<IPatientFileRepository>();
            var _therapistRepositoryMock = new Mock<ITherapistRepository>();
            var _treatmentPlanRepositoryMock = new Mock<ITreatmentPlanRepository>();
            var _httpContextMock = new Mock<HttpContext>();

            PatientController ctrl = new PatientController(_logger.Object, _userManagerMock.Object, _patientRepositoryMock.Object, 
                _patientFileRepositoryMock.Object, _remarkRepositoryMock.Object, _therapistRepositoryMock.Object, 
                _treatmentPlanRepositoryMock.Object);

            MockHttpSession mockSession = new MockHttpSession();
            mockSession["WebApiToken"] = @"""mocktoken""";
            _httpContextMock.Setup(s => s.Session).Returns(mockSession);
            ctrl.ControllerContext.HttpContext = _httpContextMock.Object;

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()).Result)
                .Returns(IdentityResult.Success);

            _patientRepositoryMock.Setup(x => x.AddPatient(It.IsAny<Patient>()))
                .Returns((true, _dummySession.Patient));
            _therapistRepositoryMock.Setup(x => x.GetByName(_dummySession.Therapist.Name))
                .Returns((_dummySession.Therapist, null));

            // Set patient age to be higher than 16
            _dummyPatientModel.DateOfBirth = new DateTime(1990, 1, 1);

            // Act
            ValidationResult validationResult = new Age(16).GetValidationResult(_dummyPatientModel.DateOfBirth, new ValidationContext(_dummyPatientModel.DateOfBirth));
            if (validationResult != ValidationResult.Success)
                ctrl.ModelState.AddModelError(nameof(_dummyPatientModel.DateOfBirth), validationResult.ErrorMessage);

            IActionResult result = ctrl.Add(_dummyPatientModel).Result;

            // Assert
            Assert.NotNull(result);
            Assert.True(ctrl.ModelState.IsValid);
            Assert.Equal(0, ctrl.ModelState.ErrorCount);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
            Assert.Equal("Info", (result as RedirectToActionResult).ControllerName);
            Assert.Equal("Account Register - Success", (result as RedirectToActionResult).RouteValues[nameof(InfoModel.Title)]);
        }

        [Fact]
        public void Patient_Add_Should_Return_View_With_No_Errors_When_Age_Equals_16()
        {
            // Happy => Patient has an age of exactly 16

            // Arrange
            var _logger = new Mock<ILogger<PatientController>>();
            var _userManagerMock = MockUserManager(new List<IdentityUser>
            {
                new IdentityUser() { Id = "1", UserName = _dummySession.Patient.Name, Email = _dummySession.Patient.Email}
            });
            var _remarkRepositoryMock = new Mock<IRemarkRepository>();
            var _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
            var _patientRepositoryMock = new Mock<IPatientRepository>();
            var _patientFileRepositoryMock = new Mock<IPatientFileRepository>();
            var _therapistRepositoryMock = new Mock<ITherapistRepository>();
            var _treatmentPlanRepositoryMock = new Mock<ITreatmentPlanRepository>();
            var _httpContextMock = new Mock<HttpContext>();

            PatientController ctrl = new PatientController(_logger.Object, _userManagerMock.Object, _patientRepositoryMock.Object,
                _patientFileRepositoryMock.Object, _remarkRepositoryMock.Object, _therapistRepositoryMock.Object,
                _treatmentPlanRepositoryMock.Object);

            MockHttpSession mockSession = new MockHttpSession();
            mockSession["WebApiToken"] = @"""mocktoken""";
            _httpContextMock.Setup(s => s.Session).Returns(mockSession);
            ctrl.ControllerContext.HttpContext = _httpContextMock.Object;

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()).Result)
                .Returns(IdentityResult.Success);

            _patientRepositoryMock.Setup(x => x.AddPatient(It.IsAny<Patient>()))
                .Returns((true, _dummySession.Patient));
            _therapistRepositoryMock.Setup(x => x.GetByName(_dummySession.Therapist.Name))
                .Returns((_dummySession.Therapist, null));

            // Set patient age to be 16
            _dummyPatientModel.DateOfBirth = new DateTime(DateTime.Now.Year - 16, DateTime.Now.Month, DateTime.Now.Day);

            // Act
            ValidationResult validationResult = new Age(16).GetValidationResult(_dummyPatientModel.DateOfBirth, new ValidationContext(_dummyPatientModel.DateOfBirth));
            if (validationResult != ValidationResult.Success)
                ctrl.ModelState.AddModelError(nameof(_dummyPatientModel.DateOfBirth), validationResult.ErrorMessage);

            IActionResult result = ctrl.Add(_dummyPatientModel).Result;

            // Assert
            Assert.NotNull(result);
            Assert.True(ctrl.ModelState.IsValid);
            Assert.Equal(0, ctrl.ModelState.ErrorCount);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
            Assert.Equal("Info", (result as RedirectToActionResult).ControllerName);
            Assert.Equal("Account Register - Success", (result as RedirectToActionResult).RouteValues[nameof(InfoModel.Title)]);
        }

        [Fact]
        public void Patient_Add_Should_Return_View_With_One_Error_When_Age_Under_16()
        {
            // Unhappy => Patient is under the age of 16

            // Arrange
            var _logger = new Mock<ILogger<PatientController>>();
            var _userManagerMock = MockUserManager(new List<IdentityUser>
            {
                new IdentityUser() { Id = "1", UserName = _dummySession.Patient.Name, Email = _dummySession.Patient.Email}
            });
            var _remarkRepositoryMock = new Mock<IRemarkRepository>();
            var _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
            var _patientRepositoryMock = new Mock<IPatientRepository>();
            var _patientFileRepositoryMock = new Mock<IPatientFileRepository>();
            var _therapistRepositoryMock = new Mock<ITherapistRepository>();
            var _treatmentPlanRepositoryMock = new Mock<ITreatmentPlanRepository>();
            var _httpContextMock = new Mock<HttpContext>();

            PatientController ctrl = new PatientController(_logger.Object, _userManagerMock.Object, _patientRepositoryMock.Object,
                _patientFileRepositoryMock.Object, _remarkRepositoryMock.Object, _therapistRepositoryMock.Object,
                _treatmentPlanRepositoryMock.Object);

            MockHttpSession mockSession = new MockHttpSession();
            mockSession["WebApiToken"] = @"""mocktoken""";
            _httpContextMock.Setup(s => s.Session).Returns(mockSession);
            ctrl.ControllerContext.HttpContext = _httpContextMock.Object;

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()).Result)
                .Returns(IdentityResult.Success);

            _patientRepositoryMock.Setup(x => x.AddPatient(It.IsAny<Patient>()))
                .Returns((true, _dummySession.Patient));
            _therapistRepositoryMock.Setup(x => x.GetByName(_dummySession.Therapist.Name))
                .Returns((_dummySession.Therapist, null));

            // Set patient age to be lower than 16
            _dummyPatientModel.DateOfBirth = new DateTime(2014, 1, 1);

            // Act
            ValidationResult validationResult = new Age(16).GetValidationResult(_dummyPatientModel.DateOfBirth, new ValidationContext(_dummyPatientModel.DateOfBirth));        
            if (validationResult != ValidationResult.Success)
                ctrl.ModelState.AddModelError(nameof(_dummyPatientModel.DateOfBirth), validationResult.ErrorMessage);

            ActionResult result = ctrl.Add(_dummyPatientModel).Result as ActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(ctrl.ModelState.IsValid);
            Assert.Equal(1, ctrl.ModelState.ErrorCount);
            Assert.True(ctrl.ModelState.ContainsKey(nameof(PatientModel.DateOfBirth)));
            Assert.EndsWith($"must have an age minimum of 16",
                ctrl.ModelState[nameof(PatientModel.DateOfBirth)].Errors.First().ErrorMessage);
        }
    }
}