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
    [Route("api/v1/diagnoses")]
    [ApiController]
    [ResponseCache(VaryByHeader = "User-Agent", Duration = 60)]
    public class DiagnosesController : ControllerBase
    {
        private readonly ILogger<DiagnosesController> _logger;
        private readonly IDiagnoseRepository _diagnoseRepository;

        public DiagnosesController(ILogger<DiagnosesController> logger, IDiagnoseRepository diagnoseRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _diagnoseRepository = diagnoseRepository ?? throw new ArgumentNullException(nameof(diagnoseRepository));
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string code = null)
        {
            try
            {
                if (code != null)
                {
                    return Ok(_diagnoseRepository.Get(code));
                }
                else
                {
                    return Ok(_diagnoseRepository.GetAll());
                }
            }
            catch (Exception e)
            {
                return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [HttpPost]
        public IActionResult Create([FromBody] Diagnose diagnose = null)
        {
            try
            {
                if (diagnose != null)
                {
                    Diagnose created = _diagnoseRepository.Create(diagnose);
                    if (created != null)
                        return Ok(created);
                    else
                        return new JsonResult(new { Message = "Error while creating the Diagnose" }) { StatusCode = StatusCodes.Status500InternalServerError };

                }
                return new JsonResult(new { Message = "Diagnose body is empty" }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
            catch (Exception e)
            {
                return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [HttpPut]
        public IActionResult Update([FromBody] Diagnose diagnose = null)
        {
            try
            {
                if (diagnose != null)
                {
                    if (diagnose.Id.HasValue)
                    {
                        Diagnose created = _diagnoseRepository.Alter(diagnose);
                        if (created != null)
                            return Ok(created);
                        else
                            return new JsonResult(new { Message = "Error while updating the Diagnose" }) { StatusCode = StatusCodes.Status500InternalServerError };
                    } 
                    else
                        return new JsonResult(new { Message = "Diagnose ID is empty" }) { StatusCode = StatusCodes.Status500InternalServerError };
                }
                return new JsonResult(new { Message = "Diagnose body is empty" }) { StatusCode = StatusCodes.Status500InternalServerError };
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
                    int diagnoseId = -1;
                    if (int.TryParse(id, out diagnoseId))
                        if (diagnoseId > -1)
                            return Ok(_diagnoseRepository.Delete(diagnoseId));
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