using Core.Domain;
using Core.DomainServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Web_App.Models;
using Web_App.Session;

namespace Web_App.Controllers
{
    public class TreatmentController : Controller
    {
        private readonly ILogger<TreatmentController> _logger;
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IPatientRepository _patientRepository;
        public TreatmentController(ILogger<TreatmentController> logger, ITreatmentRepository treatmentRepository, IPatientRepository patientRepository)
        {
            _logger = logger;
            _treatmentRepository = treatmentRepository;
            _patientRepository = patientRepository;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        [Route("/Treatment/Add/{tpid?}/{pn?}")]
        public async Task<IActionResult> Add(string tpid, string pn)
        {
            InfoModel infoModel = new InfoModel();
            int treatmentPlanId = -1;
            if (!string.IsNullOrEmpty(pn) && int.TryParse(tpid, out treatmentPlanId))
            {
                TreatmentModel treatmentModel = new TreatmentModel();
                treatmentModel.PatientName = _patientRepository.Get(pn).Name;
                treatmentModel.PatientNumber = pn;
                treatmentModel.TreatmentPlanId = treatmentPlanId;
                treatmentModel.AddedDate = DateTime.Now;
                treatmentModel.EndDate = DateTime.Now.AddDays(7);
                treatmentModel.TreatmentTypes = new List<TreatmentType>();

                if (!string.IsNullOrEmpty(HttpContext.Session.GetObject<string>("WebApiToken")))
                {
                    using (HttpClient http = new HttpClient())
                    {
                        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetObject<string>("WebApiToken"));
                        treatmentModel.TreatmentTypes = await http.GetFromJsonAsync<List<TreatmentType>>($"{HttpContext.Session.GetObject<string>("WebAPIHost")}/api/v1/treatmenttypes");
                    }
                }
                return View(treatmentModel);
            }
            else
            {
                infoModel.TableEntries.Add("TPID Parameter: ", tpid);
                infoModel.TableEntries.Add("PN Parameter: ", pn);
            }
            infoModel.Title = "Add Treatment - Error";
            infoModel.Description = $"An error occurred while trying to view the Treatment";
            infoModel.Description += "\n, please contact the server admins to resolve this issue.";
            infoModel.HasImportantData = false;

            infoModel.ReturnAction = "Index";
            infoModel.ReturnController = "Patient";

            HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
            return RedirectToAction("Index", "Info", infoModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        [Route("/Treatment/Add/{tpid?}/{pn?}")]
        public async Task<IActionResult> Add(TreatmentModel treatmentModel, string tpid, string pn)
        {
            if (ModelState.IsValid)
            {
                Treatment forgedTreatment = treatmentModel.Forge();
                string token = HttpContext.Session.GetObject<string>("WebApiToken");
                if (!string.IsNullOrEmpty(token))
                {
                    if (token != "mocktoken")
                    {
                        using (HttpClient http = new HttpClient())
                        {
                            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            treatmentModel.TreatmentTypes = await http.GetFromJsonAsync<List<TreatmentType>>($"{HttpContext.Session.GetObject<string>("WebAPIHost")}/api/v1/treatmenttypes?code=" + forgedTreatment.VektisType);
                        }
                    }
                    if (treatmentModel.TreatmentTypes.Count() > 0)
                    {
                        if (treatmentModel.TreatmentTypes.FirstOrDefault(x => x.Code == forgedTreatment.VektisType).IsExplanationMandatory && string.IsNullOrEmpty(forgedTreatment.Particularities))
                            ModelState.AddModelError("Particularities", "Particularities is required for this Treatment Type");
                        else
                        {
                            (bool, Exception) addTreatmentResult = _treatmentRepository.AddTreatment(forgedTreatment);
                            if (addTreatmentResult.Item1)
                                return RedirectToAction("Edit", "Patient", new { pn = treatmentModel.PatientNumber, tabTo = "tmp-tab" });
                        }
                    }
                }
            } 
            else
            {
                if (!string.IsNullOrEmpty(HttpContext.Session.GetObject<string>("WebApiToken")))
                {
                    using (HttpClient http = new HttpClient())
                    {
                        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetObject<string>("WebApiToken"));
                        treatmentModel.TreatmentTypes = await http.GetFromJsonAsync<List<TreatmentType>>($"{HttpContext.Session.GetObject<string>("WebAPIHost")}/api/v1/treatmenttypes");
                    }
                }
            }
            treatmentModel.PatientName = _patientRepository.Get(pn).Name;
            return View(treatmentModel);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        [Route("/Treatment/Edit/{tid?}/{pn?}")]
        public async Task<IActionResult> Edit(string tid, string pn)
        {
            InfoModel infoModel = new InfoModel();
            int treatmentId = -1;
            if (!string.IsNullOrEmpty(pn) && int.TryParse(tid, out treatmentId))
            {
                Treatment treatment = _treatmentRepository.GetTreatment(treatmentId);
                if (treatment != null)
                {
                    TreatmentModel treatmentModel = TreatmentModel.Create(treatment, pn, _patientRepository.Get(pn).Name);
                    treatmentModel.PatientName = _patientRepository.Get(pn).Name;
                    treatmentModel.PatientNumber = pn;
                    treatmentModel.TreatmentTypes = new List<TreatmentType>();

                    if (!string.IsNullOrEmpty(HttpContext.Session.GetObject<string>("WebApiToken")))
                    {
                        using (HttpClient http = new HttpClient())
                        {
                            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetObject<string>("WebApiToken"));
                            treatmentModel.TreatmentTypes = await http.GetFromJsonAsync<List<TreatmentType>>($"{HttpContext.Session.GetObject<string>("WebAPIHost")}/api/v1/treatmenttypes");
                        }
                    }
                    return View(treatmentModel);
                }
                infoModel.TableEntries.Add("Treatment Entity: ", treatment == null ? "Empty" : "Set");
            }
            else
            {
                infoModel.TableEntries.Add("TID Parameter: ", tid);
                infoModel.TableEntries.Add("PN Parameter: ", pn);
            }

            infoModel.Title = "Edit Treatment - Error";
            infoModel.Description = $"An error occurred while trying to view the Treatment";
            infoModel.Description += "\n, please contact the server admins to resolve this issue.";
            infoModel.HasImportantData = false;

            infoModel.ReturnAction = "Index";
            infoModel.ReturnController = "Patient";

            HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
            return RedirectToAction("Index", "Info", infoModel);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        [Route("/Treatment/Edit/{tid?}/{pn?}")]
        public async Task<IActionResult> Edit(TreatmentModel treatmentModel, string tid, string pn)
        {
            int treatmentId = -1;
            if (ModelState.IsValid && int.TryParse(tid, out treatmentId))
            {
                Treatment forgedTreatment = treatmentModel.Forge();
                forgedTreatment.Id = treatmentId;
                treatmentModel.Id = treatmentId;
                if (!string.IsNullOrEmpty(HttpContext.Session.GetObject<string>("WebApiToken")))
                {
                    using (HttpClient http = new HttpClient())
                    {
                        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetObject<string>("WebApiToken"));
                        treatmentModel.TreatmentTypes = await http.GetFromJsonAsync<List<TreatmentType>>($"{HttpContext.Session.GetObject<string>("WebAPIHost")}/api/v1/treatmenttypes?code=" + forgedTreatment.VektisType);
                    }
                    if (treatmentModel.TreatmentTypes.Count() > 0)
                    {
                        if (treatmentModel.TreatmentTypes.FirstOrDefault(x => x.Code == forgedTreatment.VektisType).IsExplanationMandatory && string.IsNullOrEmpty(forgedTreatment.Particularities))
                            ModelState.AddModelError("Particularities", "Particularities is required for this Treatment Type");
                        else
                        {
                            (bool, Exception) addTreatmentResult = _treatmentRepository.Update(forgedTreatment, treatmentId);
                        }
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(HttpContext.Session.GetObject<string>("WebApiToken")))
                {
                    using (HttpClient http = new HttpClient())
                    {
                        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetObject<string>("WebApiToken"));
                        treatmentModel.TreatmentTypes = await http.GetFromJsonAsync<List<TreatmentType>>($"{HttpContext.Session.GetObject<string>("WebAPIHost")}/api/v1/treatmenttypes");
                    }
                }
            }
            treatmentModel.PatientName = _patientRepository.Get(pn).Name;
            return View(treatmentModel);
        }

        [HttpGet]
        [Route("/Treatment/Delete/{tid?}/{pn?}")]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        public IActionResult Delete(string tid, string pn)
        {
            int treatmentId = -1;
            if (!string.IsNullOrEmpty(pn) && int.TryParse(tid, out treatmentId))
            {
                Treatment treatment = _treatmentRepository.GetTreatment(treatmentId);
                if (treatment != null)
                {
                    (bool, Exception) tmpResult = _treatmentRepository.Delete(treatment.Id);
                    if (!tmpResult.Item1)
                    {
                        InfoModel infoModel = new InfoModel();
                        infoModel.Title = "Delete Treatment - Error";
                        infoModel.Description = $"An error occurred while trying to delete the Treatment";
                        infoModel.Description += "\n, please contact the server admins to resolve this issue.";
                        infoModel.HasImportantData = false;

                        if (tmpResult.Item2 != null)
                            infoModel.TableEntries.Add(tmpResult.Item2.GetType().Name, tmpResult.Item2.Message);

                        infoModel.ReturnAction = "Index";
                        infoModel.ReturnController = "Patient";

                        HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                        return RedirectToAction("Index", "Info", infoModel);
                    }
                }
                else
                {
                    InfoModel infoModel = new InfoModel();
                    infoModel.Title = "Delete Treatment - Error";
                    infoModel.Description = $"An error occurred while trying to delete the Treatment";
                    infoModel.Description += "\n, please contact the server admins to resolve this issue.";
                    infoModel.HasImportantData = false;

                    infoModel.TableEntries.Add("Message", "No Treatment found");

                    infoModel.ReturnAction = "Index";
                    infoModel.ReturnController = "Patient";

                    HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                    return RedirectToAction("Index", "Info", infoModel);
                }
            }
            return RedirectToAction("Edit", "Patient", new { pn = pn, tabTo = "tmp-tab" });
        }
    }
}