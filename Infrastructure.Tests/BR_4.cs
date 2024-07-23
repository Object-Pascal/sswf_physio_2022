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
using Xunit;

namespace Infrastructure.Tests
{
    public class BR_4
    {
        private TreatmentModel _dummyTreatmentModel;
        private Treatment _dummyTreatment;
        private DummySession _dummySession;
        public BR_4()
        {
            _dummySession = new DummySession();
            _dummyTreatmentModel = new TreatmentModel()
            {
                Id = 0,
                VektisType = "1234",
                Particularities = "Mock",
                Room = "Mock",
                AddedDate = new DateTime(2022, 1, 1),
                EndDate = new DateTime(2022, 1, 8),
                TreatmentPlanId = 0,
                PatientNumber = _dummySession.Patient.PatientNumber,
                PatientName = _dummySession.Patient.Name,
                TreatmentTypes = new List<TreatmentType>()
                {
                    new TreatmentType()
                    {
                        Id = 0,
                        Code = "1234",
                        Description = "Mock",
                        IsExplanationMandatory = true
                    },
                    new TreatmentType()
                    {
                        Id = 0,
                        Code = "4321",
                        Description = "Mock",
                        IsExplanationMandatory = false
                    }
                }
            };

            _dummyTreatment = new Treatment()
            {
                Id = 1,
                VektisType = _dummyTreatmentModel.VektisType,
                Particularities = _dummyTreatmentModel.Particularities,
                Room = _dummyTreatmentModel.Room,
                AddedDate = _dummyTreatmentModel.AddedDate,
                EndDate = _dummyTreatmentModel.EndDate,
                TreatmentPlan = null,
                TreatmentPlanId = _dummyTreatmentModel.TreatmentPlanId
            };
        }

        [Fact]
        public void Treatment_Add_Should_Return_View_With_No_Errors_When_Particularities_Required_Filled_In()
        {
            // Happy => Particularities filled in.

            // Arrange
            var _logger = new Mock<ILogger<TreatmentController>>();
            var _patientRepositoryMock = new Mock<IPatientRepository>();
            var _treatmentRepositoryMock = new Mock<ITreatmentRepository>();
            var _httpContextMock = new Mock<HttpContext>();

            TreatmentController ctrl = new TreatmentController(_logger.Object, _treatmentRepositoryMock.Object, _patientRepositoryMock.Object);

            MockHttpSession mockSession = new MockHttpSession();
            mockSession["WebApiToken"] = @"""mocktoken""";
            _httpContextMock.Setup(s => s.Session).Returns(mockSession);
            ctrl.ControllerContext.HttpContext = _httpContextMock.Object;

            _patientRepositoryMock.Setup(x => x.Get(_dummySession.Patient.PatientNumber))
                .Returns(_dummySession.Patient);
            _treatmentRepositoryMock.Setup(x => x.AddTreatment(It.IsAny<Treatment>()))
                .Returns((true, null));

            // Particularities filled -> required for this vektistype
            _dummyTreatmentModel.VektisType = "1234";
            _dummyTreatmentModel.Particularities = "Mock Particularities";

            // Act
            ActionResult result = ctrl.Add(_dummyTreatmentModel, _dummyTreatmentModel.TreatmentPlanId.ToString(), _dummyTreatmentModel.PatientNumber).Result as ActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(ctrl.ModelState.IsValid);
            Assert.Equal(0, ctrl.ModelState.ErrorCount);
        }

        [Fact]
        public void Treatment_Add_Should_Return_View_With_No_Errors_When_Particularities_Not_Required_Not_Filled_In()
        {
            // Happy => Particularities filled in.

            // Arrange
            var _logger = new Mock<ILogger<TreatmentController>>();
            var _patientRepositoryMock = new Mock<IPatientRepository>();
            var _treatmentRepositoryMock = new Mock<ITreatmentRepository>();
            var _httpContextMock = new Mock<HttpContext>();

            TreatmentController ctrl = new TreatmentController(_logger.Object, _treatmentRepositoryMock.Object, _patientRepositoryMock.Object);

            MockHttpSession mockSession = new MockHttpSession();
            mockSession["WebApiToken"] = @"""mocktoken""";
            _httpContextMock.Setup(s => s.Session).Returns(mockSession);
            ctrl.ControllerContext.HttpContext = _httpContextMock.Object;

            _patientRepositoryMock.Setup(x => x.Get(_dummySession.Patient.PatientNumber))
                .Returns(_dummySession.Patient);
            _treatmentRepositoryMock.Setup(x => x.AddTreatment(It.IsAny<Treatment>()))
                .Returns((true, null));

            // Particularities empty -> not required for this vektistype
            _dummyTreatmentModel.VektisType = "4321";
            _dummyTreatmentModel.Particularities = string.Empty;

            // Act
            ActionResult result = ctrl.Add(_dummyTreatmentModel, _dummyTreatmentModel.TreatmentPlanId.ToString(), _dummyTreatmentModel.PatientNumber).Result as ActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(ctrl.ModelState.IsValid);
            Assert.Equal(0, ctrl.ModelState.ErrorCount);
        }

        [Fact]
        public void Treatment_Add_Should_Return_View_With_One_Error_When_Particularities_Required_Not_Filled_In()
        {
            // Unhappy => Particularities not filled in.

            // Arrange
            var _logger = new Mock<ILogger<TreatmentController>>();
            var _patientRepositoryMock = new Mock<IPatientRepository>();
            var _treatmentRepositoryMock = new Mock<ITreatmentRepository>();
            var _httpContextMock = new Mock<HttpContext>();

            TreatmentController ctrl = new TreatmentController(_logger.Object, _treatmentRepositoryMock.Object, _patientRepositoryMock.Object);

            MockHttpSession mockSession = new MockHttpSession();
            mockSession["WebApiToken"] = @"""mocktoken""";
            _httpContextMock.Setup(s => s.Session).Returns(mockSession);
            ctrl.ControllerContext.HttpContext = _httpContextMock.Object;

            _patientRepositoryMock.Setup(x => x.Get(_dummySession.Patient.PatientNumber))
                .Returns(_dummySession.Patient);
            _treatmentRepositoryMock.Setup(x => x.AddTreatment(It.IsAny<Treatment>()))
                .Returns((true, null));

            // Particularities empty -> required for this vektistype
            _dummyTreatmentModel.VektisType = "1234";
            _dummyTreatmentModel.Particularities = string.Empty;

            // Act
            ActionResult result = ctrl.Add(_dummyTreatmentModel, _dummyTreatmentModel.TreatmentPlanId.ToString(), _dummyTreatmentModel.PatientNumber).Result as ActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(ctrl.ModelState.IsValid);
            Assert.Equal(1, ctrl.ModelState.ErrorCount);
            Assert.True(ctrl.ModelState.ContainsKey(nameof(TreatmentModel.Particularities)));
            Assert.Equal("Particularities is required for this Treatment Type",
                ctrl.ModelState[nameof(TreatmentModel.Particularities)].Errors.First().ErrorMessage);
        }
    }
}