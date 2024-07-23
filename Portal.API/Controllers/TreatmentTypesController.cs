using Core.Domain;
using Core.DomainServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace Portal.API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v1/treatmenttypes")]
    [ApiController]
    [ResponseCache(VaryByHeader = "User-Agent", Duration = 60)]
    public class TreatmentTypesController : ControllerBase
    {
        private readonly ILogger<TreatmentTypesController> _logger;
        private readonly ITreatmentTypeRepository _treatmentTypeRepository;

        public TreatmentTypesController(ILogger<TreatmentTypesController> logger, ITreatmentTypeRepository treatmentTypeRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _treatmentTypeRepository = treatmentTypeRepository ?? throw new ArgumentNullException(nameof(treatmentTypeRepository));
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string code = null)
        {
            try
            {
                if (code != null)
                {
                    return Ok(_treatmentTypeRepository.Get(code));
                }
                else
                {
                    return Ok(_treatmentTypeRepository.GetAll());
                }
            } 
            catch (Exception e)
            {
                return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [HttpPost]
        public IActionResult Create([FromBody] TreatmentType treatmentType = null)
        {
            try
            {
                if (treatmentType != null)
                {
                    TreatmentType created = _treatmentTypeRepository.Create(treatmentType);
                    if (created != null)
                        return Ok(created);
                    else
                        return new JsonResult(new { Message = "Error while creating the TreatmentType" }) { StatusCode = StatusCodes.Status500InternalServerError };

                }
                return new JsonResult(new { Message = "TreatmentType body is empty" }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
            catch (Exception e)
            {
                return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [HttpPut]
        public IActionResult Update([FromBody] TreatmentType treatmentType = null)
        {
            try
            {
                if (treatmentType != null)
                {
                    if (treatmentType.Id.HasValue)
                    {
                        TreatmentType created = _treatmentTypeRepository.Alter(treatmentType);
                        if (created != null)
                            return Ok(created);
                        else
                            return new JsonResult(new { Message = "Error while updating the TreatmentType" }) { StatusCode = StatusCodes.Status500InternalServerError };
                    }
                    else
                        return new JsonResult(new { Message = "TreatmentType ID is empty" }) { StatusCode = StatusCodes.Status500InternalServerError };
                }
                return new JsonResult(new { Message = "TreatmentType body is empty" }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
            catch (Exception e)
            {
                return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [HttpDelete]
        public IActionResult Delete([FromQuery] string id = null)
        {
            try
            {
                if (id != null)
                {
                    int treatmentTypeId = -1;
                    if (int.TryParse(id, out treatmentTypeId))
                        if (treatmentTypeId > -1)
                            return Ok(_treatmentTypeRepository.Delete(treatmentTypeId));
                        else
                            return new JsonResult(new { Message = "Error while parsing ID" }) { StatusCode = StatusCodes.Status500InternalServerError };

                }
                return new JsonResult(new { Message = "ID parameter is empty" }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
            catch (Exception e)
            {
                return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }
    }
}