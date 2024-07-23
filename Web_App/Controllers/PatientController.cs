using Core.Domain;
using Core.DomainServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Web_App.Models;
using Web_App.Session;

namespace Web_App.Controllers
{
    public class PatientController : Controller
    {
        private readonly ILogger<PatientController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPatientRepository _patientRepository;
        private readonly IPatientFileRepository _patientFileRepository;
        private readonly IRemarkRepository _remarkRepository;
        private readonly ITherapistRepository _therapistRepository;
        private readonly ITreatmentPlanRepository _treatmentPlanRepository;

        public PatientController(ILogger<PatientController> logger, UserManager<IdentityUser> userManager, IPatientRepository patientRepository, IPatientFileRepository patientFileRepository, IRemarkRepository remarkRepository, ITherapistRepository therapistRepository, ITreatmentPlanRepository treatmentPlanRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _patientRepository = patientRepository;
            _patientFileRepository = patientFileRepository;
            _remarkRepository = remarkRepository;
            _therapistRepository = therapistRepository;
            _treatmentPlanRepository = treatmentPlanRepository;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        public IActionResult Index()
        {
            IEnumerable<Patient> patients = _patientRepository.GetAll();
            if (patients != null)
            {
                return View(new PatientsModel(patients));
            }

            InfoModel infoModel = new InfoModel();
            infoModel.Title = "View Patients - Error";
            infoModel.Description = $"An error occurred while trying to view the Patients";
            infoModel.Description += "\n, please contact the server admins to resolve this issue.";
            infoModel.HasImportantData = false;

            infoModel.TableEntries.Add("Patient List: ", patients == null ? "Empty" : "Set");

            infoModel.ReturnAction = "Index";
            infoModel.ReturnController = "Home";

            HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
            return RedirectToAction("Index", "Info", infoModel);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        public async Task<IActionResult> Add()
        {
            PatientModel patientModel = new PatientModel();
            patientModel.Therapists = _therapistRepository.GetAll();
            patientModel.PatientNumber = Guid.NewGuid().ToString();
            patientModel.RegisterDate = DateTime.Now;

            patientModel.HasResignDate = false;
            patientModel.ResignDate = DateTime.Now.AddDays(1);
            patientModel.VektisDiagnoses = new List<Diagnose>();

            if (!string.IsNullOrEmpty(HttpContext.Session.GetObject<string>("WebApiToken")))
            {
                using (HttpClient http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetObject<string>("WebApiToken"));
                    patientModel.VektisDiagnoses = await http.GetFromJsonAsync<List<Diagnose>>($"{HttpContext.Session.GetObject<string>("WebAPIHost")}/api/v1/diagnoses");
                }
            }
            return View(patientModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        public async Task<IActionResult> Add(PatientModel patientModel)
        {
            if (ModelState.IsValid)
            {
                (Patient, PatientFile) patientDetails = patientModel.Forge(_therapistRepository, PatientModel.PlaceHolderAvatarBase64);
                (bool, Patient) newPatientResult = _patientRepository.AddPatient(patientDetails.Item1);
                if (newPatientResult.Item1)
                {
                    ApplicationUser newPatientUser = new ApplicationUser();
                    newPatientUser.UserName = patientDetails.Item1.Email.Replace(" ", "");
                    newPatientUser.DataId = newPatientResult.Item2.Id;
                    string newPassword = Path.GetRandomFileName();

                    IdentityResult registerResult = await _userManager.CreateAsync(newPatientUser, newPassword);
                    InfoModel infoModel = new InfoModel();
                    if (!registerResult.Succeeded)
                    {
                        infoModel.Title = "Account Register - Error";
                        infoModel.Description = $"An error occurred while trying to register patient login <b>{newPatientUser.UserName}</b> with password <b>{newPassword}</b>";
                        infoModel.Description += "\n, please contact the server admins to resolve this issue.";

                        foreach (IdentityError e in registerResult.Errors)
                            infoModel.TableEntries.Add(e.Code, e.Description);
                    }
                    else
                    {
                        IdentityResult roleResult = await _userManager.AddToRoleAsync(newPatientUser, Role.PATIENT_ROLE);
                        if (!roleResult.Succeeded)
                        {
                            infoModel.Title = "Account Register - Error";
                            infoModel.Description = $"An error occurred while trying to add the new patient login <b>{newPatientUser.UserName}</b> with password <b>{newPassword}</b> to the patient role";
                            infoModel.Description += "\n, please contact the server admins to resolve this issue.";

                            foreach (IdentityError e in roleResult.Errors)
                                infoModel.TableEntries.Add(e.Code, e.Description);

                            HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                            return RedirectToAction("Index", "Info", infoModel);
                        }

                        infoModel.Title = "Account Register - Success";
                        infoModel.Description = $"Successfully added a new patient <b>{patientDetails.Item1.Name}</b>.";
                        infoModel.Description += $"\nSuccessfully registered <b>{newPatientUser.UserName}</b> using the following credentials:";
                        infoModel.HasImportantData = true;

                        infoModel.TableEntries.Add("UserName (E-mail): ", newPatientUser.UserName);
                        infoModel.TableEntries.Add("Password: ", newPassword);
                    }
                    infoModel.ReturnAction = "Index";
                    infoModel.ReturnController = "Patient";

                    HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                    return RedirectToAction("Index", "Info", infoModel);
                }
            }
            patientModel.Therapists = _therapistRepository.GetAll();

            string token = HttpContext.Session.GetObject<string>("WebApiToken");
            if (token != "mocktoken")
            {
                if (!string.IsNullOrEmpty(token))
                {
                    using (HttpClient http = new HttpClient())
                    {
                        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetObject<string>("WebApiToken"));
                        patientModel.VektisDiagnoses = await http.GetFromJsonAsync<List<Diagnose>>($"{HttpContext.Session.GetObject<string>("WebAPIHost")}/api/v1/diagnoses");
                    }
                }
            }
            return View(patientModel);
        }

        [HttpGet]
        [Route("/Patient/Edit/{pn?}/{tabTo?}")]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        public async Task<IActionResult> Edit(string pn, string tabTo)
        {
            Patient patientDetails = _patientRepository.Get(pn);
            PatientFile patientFile = _patientFileRepository.Get(pn);
            patientFile.IntakeBy = _therapistRepository.Get(patientFile.IntakeById.Value);
            patientFile.HeadOfTreatment = _therapistRepository.Get(patientFile.HeadOfTreatmentId.Value);
            patientFile.UnderSupervisionBy = patientFile.UnderSupervisionById.HasValue ? _therapistRepository.Get(patientFile.UnderSupervisionById.Value) : null;

            InfoModel infoModel = new InfoModel();
            infoModel.Title = "Edit Patient - Error";
            infoModel.Description = $"An error occurred while trying to open the editor for the patient";
            infoModel.Description += "\n, please contact the server admins to resolve this issue.";
            infoModel.HasImportantData = false;

            infoModel.ReturnAction = "Index";
            infoModel.ReturnController = "Patient";

            if (patientDetails != null && patientFile != null)
            {
                PatientModel patientModel = PatientModel.Create(patientDetails, patientFile);
                patientModel.Therapists = _therapistRepository.GetAll();

                TreatmentPlan treatmentPlan = _treatmentPlanRepository.Get(patientDetails.PatientNumber);
                TreatmentPlanModel treatmentPlanModel = null;
                if (treatmentPlan != null)
                {
                    if (treatmentPlan.Treatments == null)
                        treatmentPlan.Treatments = _treatmentPlanRepository.GetTreatments(treatmentPlan.Id);
                    if (treatmentPlan.PatientFile == null)
                        treatmentPlan.PatientFile = patientFile;
                    if (treatmentPlan.UnderTreatmentBy == null)
                        treatmentPlan.UnderTreatmentBy = _therapistRepository.Get(treatmentPlan.UnderTreatmentById.Value);

                    treatmentPlanModel = TreatmentPlanModel.Create(treatmentPlan);
                    treatmentPlanModel.TreatmentPlanState = TreatmentPlanState.Set;
                }
                else
                {
                    int currUserId = HttpContext.Session.GetObject<int>("UserDataId");
                    Therapist currTherapist = _therapistRepository.Get(currUserId);
                    treatmentPlanModel = TreatmentPlanModel.CreateDefault(currTherapist, patientModel.PatientNumber);
                    treatmentPlanModel.TreatmentPlanState = TreatmentPlanState.Empty;
                }
                treatmentPlanModel.Therapists = patientModel.Therapists;

                RemarkModel remarkModel = new RemarkModel();
                remarkModel.Remarks = _remarkRepository.GetAll(pn) ?? new List<Remark>();

                if (!string.IsNullOrEmpty(HttpContext.Session.GetObject<string>("WebApiToken")))
                {
                    using (HttpClient http = new HttpClient())
                    {
                        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetObject<string>("WebApiToken"));
                        patientModel.VektisDiagnoses = await http.GetFromJsonAsync<List<Diagnose>>($"{HttpContext.Session.GetObject<string>("WebAPIHost")}/api/v1/diagnoses");
                    }
                }

                if (TempData.ContainsKey("ModelState.ErrorList") && TempData.ContainsKey("ModelState.ErrorAttemptedValues"))
                {
                    Dictionary<string, string> errors = TempData["ModelState.ErrorList"] as Dictionary<string, string>;
                    Dictionary<string, string> errorAttemptedValues = TempData["ModelState.ErrorAttemptedValues"] as Dictionary<string, string>;
                    foreach (KeyValuePair<string, string> e in errors)
                    {
                        ModelState.AddModelError(e.Key, e.Value);
                        ModelState[e.Key].AttemptedValue = errorAttemptedValues[e.Key];
                    }
                    TempData.Remove("ModelState.ErrorList");
                    TempData.Remove("ModelState.ErrorAttemptedValues");
                }

                ViewData["TabTo"] = tabTo ?? "ed-tab";
                return View(new EditModel(patientModel, treatmentPlanModel, remarkModel));
            }
            else
            {
                infoModel.TableEntries.Add("Patient Details: ", patientDetails == null ? "Empty" : "Set");
                infoModel.TableEntries.Add("Patient File: ", patientFile == null ? "Empty" : "Set");
            }
            HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
            return RedirectToAction("Index", "Info", infoModel);
        }

        [HttpPost]
        [Route("/Patient/Edit/{pn?}/{tabTo?}")]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        public async Task<IActionResult> Edit(PatientModel patientModel, string pn, string tabTo)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(pn))
            {
                string profilePictureBase64 = _patientRepository.Get(pn).ProfilePictureBase64;
                (Patient, PatientFile) patientDetails = patientModel.Forge(_therapistRepository, profilePictureBase64);
                patientDetails.Item1.Id = _patientRepository.Get(patientModel.PatientNumber).Id;
                patientDetails.Item2.Id = _patientFileRepository.Get(patientModel.PatientNumber).Id;

                patientDetails.Item1.PatientFileId = patientDetails.Item2.Id;

                TreatmentPlan tmpRes = _treatmentPlanRepository.Get(pn);
                patientDetails.Item2.TreatmentPlanId = tmpRes != null ? tmpRes.Id : null;

                (bool, Exception) patientResult = _patientRepository.Update(patientDetails.Item1);
                (bool, Exception) patientFileResult = _patientFileRepository.Update(patientDetails.Item2, patientModel.PatientNumber);

                if (!patientResult.Item1)
                {
                    InfoModel infoModel = new InfoModel();
                    infoModel.Title = "Edit Patient - Error";
                    infoModel.Description = $"An error occurred while trying to save the edited data for the patient.";
                    infoModel.Description += "\n, please contact the server admins to resolve this issue.";
                    infoModel.HasImportantData = false;

                    if (patientResult.Item2 != null)
                        infoModel.TableEntries.Add(patientResult.Item2.GetType().Name, patientResult.Item2.Message);
                    if (patientFileResult.Item2 != null)
                        infoModel.TableEntries.Add(patientFileResult.Item2.GetType().Name, patientFileResult.Item2.Message);

                    infoModel.ReturnAction = "Index";
                    infoModel.ReturnController = "Patient";

                    HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                    return RedirectToAction("Index", "Info", infoModel);
                }
            }

            TreatmentPlan treatmentPlan = _treatmentPlanRepository.Get(patientModel.PatientNumber);
            TreatmentPlanModel treatmentPlanModel = null;
            if (treatmentPlan != null)
            {
                if (treatmentPlan.Treatments == null)
                    treatmentPlan.Treatments = _treatmentPlanRepository.GetTreatments(treatmentPlan.Id);
                if (treatmentPlan.PatientFile == null)
                    treatmentPlan.PatientFile = _patientFileRepository.Get(patientModel.PatientNumber);
                if (treatmentPlan.UnderTreatmentBy == null)
                    treatmentPlan.UnderTreatmentBy = _therapistRepository.Get(treatmentPlan.UnderTreatmentById.Value);

                treatmentPlanModel = TreatmentPlanModel.Create(treatmentPlan);
                treatmentPlanModel.TreatmentPlanState = TreatmentPlanState.Set;
            }
            else
            {
                IdentityUser currUser = HttpContext.Session.GetObject<IdentityUser>("CurrentUser");
                Therapist currTherapist = _therapistRepository.GetByName(currUser.UserName).Item1;
                treatmentPlanModel = TreatmentPlanModel.CreateDefault(currTherapist, patientModel.PatientNumber);
                treatmentPlanModel.TreatmentPlanState = TreatmentPlanState.Empty;
            }

            patientModel.Therapists = _therapistRepository.GetAll();
            treatmentPlanModel.Therapists = patientModel.Therapists;

            RemarkModel remarkModel = new RemarkModel();
            remarkModel.Remarks = _remarkRepository.GetAll(pn) ?? new List<Remark>();

            if (!string.IsNullOrEmpty(HttpContext.Session.GetObject<string>("WebApiToken")))
            {
                using (HttpClient http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetObject<string>("WebApiToken"));
                    patientModel.VektisDiagnoses = await http.GetFromJsonAsync<List<Diagnose>>($"{HttpContext.Session.GetObject<string>("WebAPIHost")}/api/v1/diagnoses");
                }
            }

            ViewData["TabTo"] = tabTo ?? "ed-tab";
            return View(new EditModel(patientModel, treatmentPlanModel, remarkModel));
        }

        [HttpPost]
        [Route("/Patient/EditAddressData/{tabTo?}")]
        [Authorize(Roles = "Administrator,Therapist,Student,Patient")]
        public IActionResult EditAddressData(PatientModel patientModel, string tabTo)
        {
            if (ModelState.IsValid)
            {
                Patient patientDetails = _patientRepository.Get(patientModel.PatientNumber);
                PatientModel modified = PatientModel.Create(patientDetails, null);
                modified.City = patientModel.City;
                modified.Street = patientModel.Street;
                modified.HouseNumber = patientModel.HouseNumber;

                Patient modifiedDetails = modified.ForgePatient(patientDetails.ProfilePictureBase64);
                modifiedDetails.Id = patientDetails.Id;
                modifiedDetails.PatientFileId = patientDetails.PatientFileId;

                (bool, Exception) patientResult = _patientRepository.Update(modifiedDetails);
                if (!patientResult.Item1)
                {
                    InfoModel infoModel = new InfoModel();
                    infoModel.Title = "Edit Patient - Error";
                    infoModel.Description = $"An error occurred while trying to save the edited data for the patient.";
                    infoModel.Description += "\n, please contact the server admins to resolve this issue.";
                    infoModel.HasImportantData = false;

                    if (patientResult.Item2 != null)
                        infoModel.TableEntries.Add(patientResult.Item2.GetType().Name, patientResult.Item2.Message);

                    infoModel.ReturnAction = "Index";
                    infoModel.ReturnController = "Patient";

                    HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                    return RedirectToAction("Index", "Info", infoModel);
                }
                return RedirectToAction("View", "Patient", new { pn = patientModel.PatientNumber, tabTo = "ed-tab" });
            }
            Dictionary<string, string> modelErrors = new Dictionary<string, string>();
            Dictionary<string, string> errorAttemptedValue = new Dictionary<string, string>();
            foreach (KeyValuePair<string, ModelStateEntry> m in ModelState)
            {
                if (m.Value.ValidationState == ModelValidationState.Invalid)
                {
                    modelErrors.Add(m.Key, m.Value.Errors[0].ErrorMessage);
                    errorAttemptedValue.Add(m.Key, m.Value.AttemptedValue);
                }
            }
            TempData["ModelState.ErrorList"] = modelErrors;
            TempData["ModelState.ErrorAttemptedValues"] = errorAttemptedValue;
            return RedirectToAction("Edit", "Patient", new { pn = patientModel.PatientNumber, tabTo = "tmp-tab" });
        }

        [HttpGet]
        [Route("/Patient/View/{pn?}/{tabTo?}")]
        [Authorize(Roles = "Administrator,Therapist,Student,Patient")]
        public IActionResult View(string pn, string tabTo)
        {
            Patient patientDetails = _patientRepository.Get(pn);
            PatientFile patientFile = _patientFileRepository.Get(pn);

            if (patientFile != null)
            {
                patientFile.IntakeBy = _therapistRepository.Get(patientFile.IntakeById.Value);
                patientFile.HeadOfTreatment = _therapistRepository.Get(patientFile.HeadOfTreatmentId.Value);
                patientFile.UnderSupervisionBy = patientFile.UnderSupervisionById.HasValue ? _therapistRepository.Get(patientFile.UnderSupervisionById.Value) : null;
            }

            InfoModel infoModel = new InfoModel();
            infoModel.Title = "View Patient - Error";
            infoModel.Description = $"An error occurred while trying to open the editor for the patient";
            infoModel.Description += "\n, please contact the server admins to resolve this issue.";
            infoModel.HasImportantData = false;

            infoModel.ReturnAction = "Index";
            infoModel.ReturnController = "Patient";

            if (patientDetails != null && patientFile != null)
            {
                PatientModel patientModel = PatientModel.Create(patientDetails, patientFile);
                patientModel.Therapists = _therapistRepository.GetAll();

                TreatmentPlan treatmentPlan = _treatmentPlanRepository.Get(patientDetails.PatientNumber);
                TreatmentPlanModel treatmentPlanModel = null;
                if (treatmentPlan != null)
                {
                    if (treatmentPlan.Treatments == null)
                        treatmentPlan.Treatments = _treatmentPlanRepository.GetTreatments(treatmentPlan.Id);
                    if (treatmentPlan.PatientFile == null)
                        treatmentPlan.PatientFile = patientFile;

                    treatmentPlanModel = TreatmentPlanModel.Create(treatmentPlan);
                    treatmentPlanModel.TreatmentPlanState = TreatmentPlanState.Set;
                }
                else
                {
                    if (HttpContext.Session.GetBoolean("IsPatient"))
                    {
                        treatmentPlanModel = new TreatmentPlanModel();
                        treatmentPlanModel.TreatmentPlanState = TreatmentPlanState.Empty;
                    }
                    else
                    {
                        int currUserId = HttpContext.Session.GetObject<int>("UserDataId");
                        Therapist currTherapist = _therapistRepository.Get(currUserId);
                        treatmentPlanModel = TreatmentPlanModel.CreateDefault(currTherapist, patientModel.PatientNumber);
                        treatmentPlanModel.TreatmentPlanState = TreatmentPlanState.Empty;
                    }
                }
                treatmentPlanModel.Therapists = patientModel.Therapists;

                RemarkModel remarkModel = new RemarkModel();
                remarkModel.Remarks = _remarkRepository.GetAll(pn) ?? new List<Remark>();

                ViewData["TabTo"] = tabTo ?? "ed-tab";
                return View(new EditModel(patientModel, treatmentPlanModel, remarkModel));
            }
            else
            {
                infoModel.TableEntries.Add("Patient Details: ", patientDetails == null ? "Empty" : "Set");
                infoModel.TableEntries.Add("Patient File: ", patientFile == null ? "Empty" : "Set");
            }
            HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
            return RedirectToAction("Index", "Info", infoModel);
        }

        [HttpGet]
        [Route("/Patient/Delete/{pn?}")]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        public async Task<IActionResult> Delete(string pn)
        {
            if (!string.IsNullOrEmpty(pn))
            {
                InfoModel infoModel = new InfoModel();
                infoModel.Title = "Delete Patient - Error";
                infoModel.Description = $"An error occurred while trying to delete the patient";
                infoModel.Description += "\n, please contact the server admins to resolve this issue.";
                infoModel.HasImportantData = false;

                PatientFile patientFile = _patientFileRepository.Get(pn);
                if (patientFile != null)
                {
                    int oldId = _patientRepository.Get(pn).Id;
                    (bool, Exception) patientResult = _patientRepository.Delete(pn);
                    (bool, Exception) patientFileResult = _patientFileRepository.Delete(patientFile);

                    if (!patientResult.Item1 || !patientFileResult.Item1)
                    {
                        if (patientResult.Item2 != null)
                            infoModel.TableEntries.Add(patientResult.Item2.GetType().Name, patientResult.Item2.Message);
                        if (patientFileResult.Item2 != null)
                            infoModel.TableEntries.Add(patientFileResult.Item2.GetType().Name, patientFileResult.Item2.Message);

                        infoModel.ReturnAction = "Index";
                        infoModel.ReturnController = "Patient";

                        HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                        return RedirectToAction("Index", "Info", infoModel);
                    }
                    else
                    {
                        if (_userManager.Users != null)
                        {
                            IQueryable<ApplicationUser> appUsers = _userManager.Users.Select(x => (ApplicationUser)x);
                            IdentityUser identityUser = appUsers.FirstOrDefault(x => x.DataId == oldId);
                            IdentityResult registerResult = await _userManager.DeleteAsync(identityUser);
                            if (!registerResult.Succeeded)
                            {
                                infoModel.Title = "Account Register - Error";
                                infoModel.Description = $"An error occurred while trying to delete the patient's login";
                                infoModel.Description += "\n, please contact the server admins to resolve this issue.";

                                foreach (IdentityError e in registerResult.Errors)
                                    infoModel.TableEntries.Add(e.Code, e.Description);
                            }
                        }
                        else
                        {
                            infoModel.Title = "Account Register - Error";
                            infoModel.Description = $"An error occurred while trying to delete the patient's login";
                            infoModel.Description += "\n, please contact the server admins to resolve this issue.";
                            infoModel.TableEntries.Add("Users", "null");
                        }
                    }
                }
                else
                {
                    infoModel.TableEntries.Add("Message", "No Patient File found for the patient");

                    infoModel.ReturnAction = "Index";
                    infoModel.ReturnController = "Patient";

                    HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                    return RedirectToAction("Index", "Info", infoModel);
                }
            }
            return RedirectToAction("Index", "Patient");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        public IActionResult AddDefaultTreatmentPlan(string pn)
        {
            IdentityUser currUser = HttpContext.Session.GetObject<IdentityUser>("CurrentUser");
            Therapist currTherapist = _therapistRepository.GetByName(currUser.UserName).Item1;
            TreatmentPlan tmp = TreatmentPlanModel.CreateDefault(currTherapist, pn).Forge(_therapistRepository);
            tmp.PatientFile = _patientFileRepository.Get(pn);
            tmp.PatientFileId = tmp.PatientFile.Id;

            (bool, Exception) result = _treatmentPlanRepository.AddTreatmentPlan(tmp);
            if (result.Item1)
                return RedirectToAction("Edit", "Patient", new { pn = pn, tabTo = "tmp-tab" });
            else
            {
                InfoModel infoModel = new InfoModel();
                infoModel.Title = "Add Treatment Plan - Error";
                infoModel.Description = $"An error occurred while trying to add the Treatment Plan";
                infoModel.Description += "\n, please contact the server admins to resolve this issue.";
                infoModel.HasImportantData = false;

                infoModel.TableEntries.Add(result.Item2.GetType().Name, result.Item2.Message);

                infoModel.ReturnAction = "Index";
                infoModel.ReturnController = "Patient";

                HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                return RedirectToAction("Index", "Info", infoModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        public IActionResult EditTreatmentPlan(TreatmentPlanModel treatmentPlanModel)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(treatmentPlanModel.PatientNumber))
            {
                IdentityUser currUser = HttpContext.Session.GetObject<IdentityUser>("CurrentUser");
                Therapist currTherapist = _therapistRepository.GetByName(currUser.UserName).Item1;
                TreatmentPlan tmp = treatmentPlanModel.Forge(_therapistRepository);
                tmp.PatientFile = _patientFileRepository.Get(treatmentPlanModel.PatientNumber);
                tmp.PatientFileId = tmp.PatientFile.Id;
                tmp.Id = _treatmentPlanRepository.Get(treatmentPlanModel.PatientNumber).Id;

                (bool, Exception) result = _treatmentPlanRepository.Update(tmp, treatmentPlanModel.PatientNumber);
                if (result.Item1)
                    return RedirectToAction("Edit", "Patient", new { pn = treatmentPlanModel.PatientNumber, tabTo = "tmp-tab" });
                else
                {
                    InfoModel infoModel = new InfoModel();
                    infoModel.Title = "Edit Treatment Plan - Error";
                    infoModel.Description = $"An error occurred while trying to edit the Treatment Plan";
                    infoModel.Description += "\n, please contact the server admins to resolve this issue.";
                    infoModel.HasImportantData = false;

                    infoModel.TableEntries.Add(result.Item2.GetType().Name, result.Item2.Message);

                    infoModel.ReturnAction = "Index";
                    infoModel.ReturnController = "Patient";

                    HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                    return RedirectToAction("Index", "Info", infoModel);
                }
            }
            Dictionary<string, string> modelErrors = new Dictionary<string, string>();
            Dictionary<string, string> errorAttemptedValue = new Dictionary<string, string>();
            foreach (KeyValuePair<string, ModelStateEntry> m in ModelState)
            {
                if (m.Value.ValidationState == ModelValidationState.Invalid)
                {
                    modelErrors.Add(m.Key, m.Value.Errors[0].ErrorMessage);
                    errorAttemptedValue.Add(m.Key, m.Value.AttemptedValue);
                }
            }
            TempData["ModelState.ErrorList"] = modelErrors;
            TempData["ModelState.ErrorAttemptedValues"] = errorAttemptedValue;
            return RedirectToAction("Edit", "Patient", new { pn = treatmentPlanModel.PatientNumber, tabTo = "tmp-tab" });
        }

        [HttpGet]
        [Route("/Patient/DeleteTreatmentPlan/{pn?}")]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        public IActionResult DeleteTMP(string pn)
        {
            if (!string.IsNullOrEmpty(pn))
            {
                TreatmentPlan treatmentPlan = _treatmentPlanRepository.Get(pn);
                if (treatmentPlan != null)
                {
                    (bool, Exception) tmpResult = _treatmentPlanRepository.Delete(pn, treatmentPlan.Id);
                    if (!tmpResult.Item1)
                    {
                        InfoModel infoModel = new InfoModel();
                        infoModel.Title = "Delete Treatment Plan - Error";
                        infoModel.Description = $"An error occurred while trying to delete the Treatment Plan";
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
                    infoModel.Title = "Delete Treatment Plan - Error";
                    infoModel.Description = $"An error occurred while trying to delete the Treatment Plan";
                    infoModel.Description += "\n, please contact the server admins to resolve this issue.";
                    infoModel.HasImportantData = false;

                    infoModel.TableEntries.Add("Message", "No Treatment Plan found for the patient");

                    infoModel.ReturnAction = "Index";
                    infoModel.ReturnController = "Patient";

                    HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                    return RedirectToAction("Index", "Info", infoModel);
                }
            }
            return RedirectToAction("Index", "Patient");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/Patient/AddRemark/{pn?}")]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        public IActionResult AddRemark(RemarkModel remarkModel, string pn)
        {
            InfoModel infoModel = new InfoModel();
            infoModel.Title = "Add Remark - Error";
            infoModel.Description = $"An error occurred while trying to add the Remark.";
            infoModel.Description += "\n, please contact the server admins to resolve this issue.";
            infoModel.HasImportantData = false;
            if (!string.IsNullOrEmpty(pn))
            {
                if (ModelState.IsValid)
                {
                    PatientFile associatedPf = _patientFileRepository.Get(pn);
                    if (associatedPf != null)
                    {
                        Remark remark = remarkModel.Forge();
                        remark.PatientFileId = associatedPf.Id;
                        remark.RemarkMadeById = HttpContext.Session.GetObject<int>("UserDataId");
                        remark.PostedOn = DateTime.Now;

                        bool newRemarkResult = _remarkRepository.AddRemark(remark);
                        if (!newRemarkResult)
                        {
                            infoModel.TableEntries.Add("Result", "No changes");

                            infoModel.ReturnAction = "Index";
                            infoModel.ReturnController = "Patient";

                            HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                            return RedirectToAction("Index", "Info", infoModel);
                        }
                    }
                }
                return RedirectToAction("Edit", "Patient", new { pn = pn, tabTo = "rm-tab" });
            }
            infoModel.TableEntries.Add("Patient Number", "null");

            infoModel.ReturnAction = "Index";
            infoModel.ReturnController = "Patient";

            HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
            return RedirectToAction("Index", "Info", infoModel);
        }
    }
}