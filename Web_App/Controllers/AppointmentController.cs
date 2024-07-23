using Core.Domain;
using Core.DomainServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web_App.Extensions;
using Web_App.Models;
using Web_App.Session;

namespace Web_App.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ILogger<AppointmentController> _logger;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IPatientFileRepository _patientFileRepository;
        private readonly ITherapistRepository _therapistRepository;
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly ITreatmentPlanRepository _treatmentPlanRepository;

        public AppointmentController(ILogger<AppointmentController> logger, IAppointmentRepository appointmentRepository, IPatientRepository patientRepository, IPatientFileRepository patientFileRepository, ITherapistRepository therapistRepository, ITreatmentRepository treatmentRepository, ITreatmentPlanRepository treatmentPlanRepository)
        {
            _logger = logger;
            _appointmentRepository = appointmentRepository;
            _patientRepository = patientRepository;
            _patientFileRepository = patientFileRepository;
            _therapistRepository = therapistRepository;
            _treatmentRepository = treatmentRepository;
            _treatmentPlanRepository = treatmentPlanRepository;
        }

        [HttpGet]
        [Route("/Appointment/Index/{pn}")]
        [Authorize(Roles = "Administrator,Patient")]
        public IActionResult Index(string pn)
        {
            IEnumerable<Appointment> appointments = _appointmentRepository.GetByPatientNumber(pn);
            if (appointments != null)
            {
                return View(AppointmentsModel.Create(appointments.ToList(), _therapistRepository, _patientRepository));
            }
            InfoModel infoModel = new InfoModel();
            infoModel.Title = "View Appointments - Error";
            infoModel.Description = $"An error occurred while trying to view the Appointments";
            infoModel.Description += "\n, please contact the server admins to resolve this issue.";
            infoModel.HasImportantData = false;

            infoModel.TableEntries.Add("Appointment List: ", appointments == null ? "Empty" : "Set");

            infoModel.ReturnAction = "Index";
            infoModel.ReturnController = "Home";

            HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
            return RedirectToAction("Index", "Info", infoModel);
        }

        [HttpGet]
        [Route("/Appointment/Index")]
        [Authorize(Roles = "Administrator,Therapist,Student,Patient")]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetBoolean("IsPatient"))
                return RedirectToAction("Index", "Appointment", new { pn = HttpContext.Session.GetObject<string>("PatientNumber") });

            IEnumerable<Appointment> appointments = _appointmentRepository.GetAll();
            if (appointments != null)
            {
                return View(AppointmentsModel.Create(appointments.ToList(), _therapistRepository, _patientRepository));
            }
            InfoModel infoModel = new InfoModel();
            infoModel.Title = "View Appointments - Error";
            infoModel.Description = $"An error occurred while trying to view the Appointments";
            infoModel.Description += "\n, please contact the server admins to resolve this issue.";
            infoModel.HasImportantData = false;

            infoModel.TableEntries.Add("Appointment List: ", appointments == null ? "Empty" : "Set");

            infoModel.ReturnAction = "Index";
            infoModel.ReturnController = "Home";

            HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
            return RedirectToAction("Index", "Info", infoModel);
        }

        [HttpGet]
        [Route("/Appointment/Select")]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        public IActionResult Select()
        {
            AppointmentModel appointmentModel = new AppointmentModel();
            appointmentModel.Patients = _patientRepository.GetAll();
            appointmentModel.PatientNumberEntry = appointmentModel.Patients.FirstOrDefault()?.PatientNumber ?? "";
            return View(appointmentModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/Appointment/Select")]
        [Authorize(Roles = "Administrator,Therapist,Student")]
        public IActionResult Select(AppointmentModel data)
        {
            if (!string.IsNullOrEmpty(data.PatientNumberEntry))
            {
                return RedirectToAction("Add", "Appointment", new { pn = data.PatientNumberEntry});
            }
            data.Patients = _patientRepository.GetByHeadOfTreatment(HttpContext.Session.GetObject<int>("UserDataId"));
            return View(data);
        }

        [HttpGet]
        [Route("/Appointment/Add/{pn?}")]
        [Authorize(Roles = "Administrator,Therapist,Student,Patient")]
        public IActionResult Add(string pn)
        {
            Patient patient = _patientRepository.Get(pn);
            if (patient != null)
            {
                if (HttpContext.Session.GetBoolean("IsPatient"))
                    if (patient.Id != HttpContext.Session.GetObject<int>("UserDataId"))
                        return RedirectToAction("Index", "Appointment", new { pn = HttpContext.Session.GetObject<string>("PatientNumber") });

                TreatmentPlan treatmentPlan = _treatmentPlanRepository.Get(patient.PatientNumber);
                if (treatmentPlan == null)
                {
                    InfoModel infoModel = new InfoModel();
                    infoModel.Title = "Add Appointment - Error";
                    infoModel.Description = $"An error occurred while trying to create an appointment for the patient";
                    infoModel.Description += "\n, please contact the server admins to resolve this issue.";
                    infoModel.HasImportantData = false;

                    infoModel.ReturnAction = "Index";
                    infoModel.ReturnController = "Appointment";

                    infoModel.TableEntries.Add("Reason: ", "Patient does not have a Treatment Plan yet.");

                    HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                    return RedirectToAction("Index", "Info", infoModel);
                }

                AppointmentModel appointmentModel = new AppointmentModel();
                appointmentModel.PatientNameEntry = patient.Name ?? "";
                appointmentModel.PatientNumberEntry = patient.PatientNumber ?? "";
                appointmentModel.TreatmentEntry = -1;
                if (patient.PatientFileId.HasValue)
                {
                    appointmentModel.Treatments = _treatmentRepository.GetAllFromPatientFile(patient.PatientFileId.Value);
                    if (appointmentModel.Treatments != null ? appointmentModel.Treatments.Count() > 0 : false)
                        appointmentModel.TreatmentEntry = appointmentModel.Treatments.First().Id;
                    else
                        appointmentModel.Treatments = new List<Treatment>();
                }

                PatientFile patientFile = _patientFileRepository.Get(patient.PatientNumber);
                if (HttpContext.Session.GetBoolean("IsPatient"))
                {
                    if (patientFile != null)
                    {
                        Therapist headOfTreatment = _therapistRepository.Get(patientFile.HeadOfTreatmentId.HasValue ? patientFile.HeadOfTreatmentId.Value : -1);
                        if (headOfTreatment != null)
                            appointmentModel.Therapist = headOfTreatment;
                        else
                            return RedirectToAction("Index", "Appointment", new { pn = patient.PatientNumber });
                    }
                    else
                        return RedirectToAction("Index", "Appointment", new { pn = patient.PatientNumber });
                }
                else
                    appointmentModel.Therapist = _therapistRepository.Get(HttpContext.Session.GetObject<int>("UserDataId"));

                if (patientFile != null)
                {
                    appointmentModel.AppointmentDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, DateTimeKind.Utc);
                    appointmentModel.AppointmentDescription = "";

                    appointmentModel.Therapists = _therapistRepository.GetAll();
                    if (!HttpContext.Session.GetBoolean("IsPatient"))
                        appointmentModel.TherapistIdEntry = HttpContext.Session.GetObject<int>("UserDataId");

                    return View(appointmentModel);
                }

            }
            return RedirectToAction("Select", "Appointment");
        }

        [HttpPost]
        [Route("/Appointment/Add/{pn?}")]
        [Authorize(Roles = "Administrator,Therapist,Student,Patient")]
        public IActionResult Add(AppointmentModel data)
        {
            if (!HttpContext.Session.GetBoolean("IsPatient"))
            {
                Patient patient = _patientRepository.Get(data.PatientNumberEntry);
                if (patient != null)
                {
                    if (ModelState.IsValid)
                    {
                        Therapist selectedTherapist = _therapistRepository.Get(data.TherapistIdEntry);
                        if (selectedTherapist != null)
                        {
                            if (data.AppointmentDateTime.Value.IsBetween(selectedTherapist.AvailableFrom.Value, selectedTherapist.AvailableTo.Value))
                            {
                                IEnumerable<Appointment> appointments = _appointmentRepository.GetByPatientNumber(patient.PatientNumber);
                                int count = appointments.Where(x => x.AppointmentDateTime.Value.IsInSameWeek(data.AppointmentDateTime.Value)).Count();
                                bool duplicate = appointments.Any(x => x.AppointmentDateTime == data.AppointmentDateTime.Value);

                                if (!duplicate)
                                {
                                    TreatmentPlan treatmentPlan = _treatmentPlanRepository.Get(patient.PatientNumber);
                                    if (count < treatmentPlan.MaxTreatmentsPerWeek)
                                    {
                                        if (data.AppointmentDateTime.Value > treatmentPlan.StartDate.Value)
                                        {
                                            if (data.AppointmentDateTime.Value < treatmentPlan.EndDate.Value)
                                            {
                                                data.TherapistId = selectedTherapist.Id;
                                                Appointment appointment = data.ForgeAppointment(_therapistRepository, _patientRepository, _treatmentRepository);
                                                (bool, Appointment) result = _appointmentRepository.Add(appointment);
                                                if (result.Item1)
                                                    return RedirectToAction("Index", "Appointment");
                                            }
                                            else
                                                ModelState.AddModelError("AppointmentDateTime", "The Date/Time is higher than the End Date of the Treatment");
                                        }
                                        else
                                            ModelState.AddModelError("AppointmentDateTime", "The Date/Time is lower than the Start Date of the Treatment");
                                    }
                                    else
                                        ModelState.AddModelError("AppointmentDateTime", "Maximum number of treatments reached in that week");
                                }
                                else
                                    ModelState.AddModelError("AppointmentDateTime", "An appointment has already been made in this timeframe");
                            }
                            else
                                ModelState.AddModelError("AppointmentDateTime", "Therapist is not available in this timeframe");
                        }
                    }

                    data.Treatments = new List<Treatment>();
                    if (patient.PatientFileId.HasValue)
                    {
                        data.Treatments = _treatmentRepository.GetAllFromPatientFile(patient.PatientFileId.Value);
                        if (data.Treatments != null ? data.Treatments.Count() > 0 : false)
                            data.TreatmentEntry = data.Treatments.First().Id;
                    }
                    data.Therapists = _therapistRepository.GetAll();
                    return View(data);
                }

                InfoModel infoModel = new InfoModel();
                infoModel.Title = "Add Appointment - Error";
                infoModel.Description = $"An error occurred while trying to create an appointment for the patient";
                infoModel.Description += "\n, please contact the server admins to resolve this issue.";
                infoModel.HasImportantData = false;

                infoModel.TableEntries.Add("Reason: ", "The patient data could not be retrieved.");

                infoModel.ReturnAction = "Select";
                infoModel.ReturnController = "Appointment";

                HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                return RedirectToAction("Index", "Info", infoModel);
            }
            else
            {
                Patient patient = _patientRepository.Get(HttpContext.Session.GetObject<int>("UserDataId"));
                if (patient != null)
                {
                    if (ModelState.IsValid)
                    {
                        PatientFile patientFile = _patientFileRepository.Get(patient.PatientNumber);
                        if (patientFile != null)
                        {
                            Therapist headOfTreatment = _therapistRepository.Get(patientFile.HeadOfTreatmentId.HasValue ? patientFile.HeadOfTreatmentId.Value : -1);
                            if (headOfTreatment != null)
                            {
                                if (data.AppointmentDateTime.Value.IsBetween(headOfTreatment.AvailableFrom.Value, headOfTreatment.AvailableTo.Value))
                                {
                                    IEnumerable<Appointment> appointments = _appointmentRepository.GetByPatientNumber(patient.PatientNumber);
                                    int count = appointments.Where(x => x.AppointmentDateTime.Value.IsInSameWeek(data.AppointmentDateTime.Value)).Count();

                                    TreatmentPlan treatmentPlan = _treatmentPlanRepository.Get(patient.PatientNumber);
                                    if (count < treatmentPlan.MaxTreatmentsPerWeek)
                                    {
                                        if (data.AppointmentDateTime.Value > treatmentPlan.StartDate.Value)
                                        {
                                            if (data.AppointmentDateTime.Value < treatmentPlan.EndDate.Value)
                                            {
                                                data.TherapistId = patientFile.HeadOfTreatmentId.Value;
                                                Appointment appointment = data.ForgeAppointment(_therapistRepository, _patientRepository, _treatmentRepository);
                                                (bool, Appointment) result = _appointmentRepository.Add(appointment);
                                                if (result.Item1)
                                                    return RedirectToAction("Index", "Appointment", new { pn = patient.PatientNumber });
                                            }
                                            else
                                                ModelState.AddModelError("AppointmentDateTime", "The Date/Time is higher than the End Date of the Treatment");
                                        }
                                        else
                                            ModelState.AddModelError("AppointmentDateTime", "The Date/Time is lower than the Start Date of the Treatment");
                                    }
                                    else
                                        ModelState.AddModelError("AppointmentDateTime", "Maximum number of treatments reached in that week");
                                }
                                else
                                    ModelState.AddModelError("AppointmentDateTime", "Your therapist is not available in this timeframe");
                            }
                        }
                        else
                            return RedirectToAction("Index", "Appointment", new { pn = patient.PatientNumber });
                    }

                    data.Treatments = new List<Treatment>();
                    if (patient.PatientFileId.HasValue)
                    {
                        data.Treatments = _treatmentRepository.GetAllFromPatientFile(patient.PatientFileId.Value);
                        if (data.Treatments != null ? data.Treatments.Count() > 0 : false)
                            data.TreatmentEntry = data.Treatments.First().Id;
                    }
                    data.Therapists = _therapistRepository.GetAll();
                    return View(data);
                }

                InfoModel infoModel = new InfoModel();
                infoModel.Title = "Add Appointment - Error";
                infoModel.Description = $"An error occurred while trying to create an appointment";
                infoModel.Description += "\n, please contact the server admins to resolve this issue.";
                infoModel.HasImportantData = false;

                infoModel.TableEntries.Add("Reason: ", "The patient data could not be retrieved.");

                infoModel.ReturnAction = "Select";
                infoModel.ReturnController = "Appointment";

                HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                return RedirectToAction("Index", "Info", infoModel);
            }
        }

        [HttpGet]
        [Route("/Appointment/Cancel/{appid?}")]
        [Authorize(Roles = "Administrator,Therapist,Student,Patient")]
        public IActionResult Cancel(string appid)
        {
            int appointmentId = -1;
            if (int.TryParse(appid, out appointmentId))
            {
                Appointment appointment = _appointmentRepository.Get(appointmentId);
                if (appointment != null)
                {
                    InfoModel infoModel = new InfoModel();
                    infoModel.Title = "Cancel Appointment - Error";
                    infoModel.Description = $"An error occurred while trying to cancel the appointment";
                    infoModel.Description += "\n, please contact the server admins to resolve this issue.";
                    infoModel.HasImportantData = false;

                    infoModel.ReturnAction = "Index";
                    infoModel.ReturnController = "Appointment";

                    Patient patient = _patientRepository.Get(appointment.PatientId.Value);
                    if (patient != null)
                    {
                        if (HttpContext.Session.GetBoolean("IsPatient"))
                        {
                            int currPatientId = HttpContext.Session.GetObject<int>("UserDataId");
                            if (appointment.PatientId == currPatientId)
                            {
                                TimeSpan timeUntilAppointment = appointment.AppointmentDateTime.Value - DateTime.Now;
                                if (timeUntilAppointment.TotalHours > 24)
                                {
                                    (bool, Exception) result = _appointmentRepository.Delete(appointment.Id);
                                    if (result.Item1)
                                    {
                                        Patient currPatient = _patientRepository.Get(currPatientId);
                                        return RedirectToAction("Index", "Appointment", new { pn =  currPatient.PatientNumber });
                                    }
                                    else
                                    {
                                        infoModel.TableEntries.Add("Error: ", result.Item2.Message);
                                    }
                                }
                                else
                                {
                                    infoModel.TableEntries.Add("Reason: ", "You are not allowed to cancel an appointment coming up within 24 hours");
                                }
                            }
                            else
                            {
                                infoModel.TableEntries.Add("Reason: ", "You are not allowed to cancel this appointment");
                            }
                            HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                            return RedirectToAction("Index", "Info", infoModel);
                        }
                        else
                        {
                            (bool, Exception) result = _appointmentRepository.Delete(appointment.Id);
                            if (result.Item1)
                            {
                                return RedirectToAction("Index", "Appointment");
                            }
                            else
                            {
                                infoModel.TableEntries.Add("Error: ", result.Item2.Message);
                            }

                            HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                            return RedirectToAction("Index", "Info", infoModel);
                        }
                    }
                }
            }
            return RedirectToAction("Index", "Appointment");
        }

        [HttpGet]
        [Route("/Appointment/Edit/{appid?}")]
        [Authorize(Roles = "Administrator,Therapist,Student,Patient")]
        public IActionResult Edit(string appid)
        {
            int appointmentId = -1;
            if (int.TryParse(appid, out appointmentId))
            {
                Appointment appointment = _appointmentRepository.Get(appointmentId);
                if (appointment != null)
                {
                    if (HttpContext.Session.GetBoolean("IsPatient"))
                    {
                        TimeSpan timeUntilAppointment = appointment.AppointmentDateTime.Value - DateTime.Now;
                        if (timeUntilAppointment.TotalHours < 24)
                        {
                            InfoModel infoModel = new InfoModel();
                            infoModel.Title = "Edit Appointment - Error";
                            infoModel.Description = $"An error occurred while trying to edit the appointment";
                            infoModel.Description += "\n, please contact the server admins to resolve this issue.";
                            infoModel.HasImportantData = false;

                            infoModel.TableEntries.Add("Reason: ", "You are not allowed to edit an appointment coming up within 24 hours");

                            infoModel.ReturnAction = "Index";
                            infoModel.ReturnController = "Appointment";

                            HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                            return RedirectToAction("Index", "Info", infoModel);
                        }
                    }
                    Patient patient = _patientRepository.Get(appointment.PatientId.Value);
                    if (patient != null)
                    {
                        if (HttpContext.Session.GetBoolean("IsPatient"))
                            if (appointment.PatientId != HttpContext.Session.GetObject<int>("UserDataId"))
                                return RedirectToAction("Index", "Appointment", new { pn = HttpContext.Session.GetObject<string>("PatientNumber") });

                        if (!HttpContext.Session.GetBoolean("IsPatient"))
                            if (appointment.TherapistId != HttpContext.Session.GetObject<int>("UserDataId"))
                                return RedirectToAction("Index", "Appointment");

                        PatientFile patientFile = _patientFileRepository.Get(patient.PatientNumber);
                        if (patientFile != null)
                        {
                            if (HttpContext.Session.GetBoolean("IsPatient"))
                            {
                                Therapist therapist = _therapistRepository.Get(appointment.TherapistId.HasValue ? appointment.TherapistId.Value : -1);
                                if (therapist != null)
                                    appointment.Therapist = therapist;
                                else
                                    return RedirectToAction("Index", "Appointment", new { pn = patient.PatientNumber });
                            }
                            else
                                appointment.Therapist = _therapistRepository.Get(HttpContext.Session.GetObject<int>("UserDataId"));

                            AppointmentModel appointmentModel = AppointmentModel.Create(appointment, patientFile.Id, _therapistRepository, _patientRepository, _treatmentRepository);
                            appointmentModel.Therapists = _therapistRepository.GetAll();

                            if (!HttpContext.Session.GetBoolean("IsPatient"))
                                appointmentModel.TherapistIdEntry = HttpContext.Session.GetObject<int>("UserDataId");

                            return View(appointmentModel);
                        }
                    }
                }
            }
            if (HttpContext.Session.GetBoolean("IsPatient"))
                return RedirectToAction("Index", "Appointment", new { pn = HttpContext.Session.GetObject<string>("PatientNumber") });
            else
                return RedirectToAction("Index", "Appointment");
        }

        [HttpPost]
        [Route("/Appointment/Edit/{appid?}")]
        [Authorize(Roles = "Administrator,Therapist,Student,Patient")]
        public IActionResult Edit(AppointmentModel data)
        {
            if (!HttpContext.Session.GetBoolean("IsPatient"))
            {
                Patient patient = _patientRepository.Get(data.PatientNumberEntry);
                if (patient != null)
                {
                    if (ModelState.IsValid)
                    {
                        Therapist selectedTherapist = _therapistRepository.Get(data.TherapistIdEntry);
                        if (selectedTherapist != null)
                        {
                            if (data.AppointmentDateTime.Value.IsBetween(selectedTherapist.AvailableFrom.Value, selectedTherapist.AvailableTo.Value))
                            {
                                IEnumerable<Appointment> appointments = _appointmentRepository.GetByPatientNumber(patient.PatientNumber);
                                int count = appointments.Where(x => x.AppointmentDateTime.Value.IsInSameWeek(data.AppointmentDateTime.Value)).Count();

                                TreatmentPlan treatmentPlan = _treatmentPlanRepository.Get(patient.PatientNumber);
                                if (data.AppointmentDateTime.Value > treatmentPlan.StartDate.Value)
                                {
                                    if (data.AppointmentDateTime.Value < treatmentPlan.EndDate.Value)
                                    {
                                        data.TherapistId = selectedTherapist.Id;
                                        Appointment appointment = data.ForgeAppointment(_therapistRepository, _patientRepository, _treatmentRepository);
                                        (bool, Exception) result = _appointmentRepository.Update(appointment);
                                        if (result.Item1)
                                            return RedirectToAction("Index", "Appointment");
                                    }
                                    else
                                        ModelState.AddModelError("AppointmentDateTime", "The Date/Time is higher than the End Date of the Treatment");
                                }
                                else
                                    ModelState.AddModelError("AppointmentDateTime", "The Date/Time is lower than the Start Date of the Treatment");
                            }
                            else
                                ModelState.AddModelError("AppointmentDateTime", "Therapist is not available in this timeframe");
                        }
                    }

                    data.Treatments = new List<Treatment>();
                    if (patient.PatientFileId.HasValue)
                    {
                        data.Treatments = _treatmentRepository.GetAllFromPatientFile(patient.PatientFileId.Value);
                        if (data.Treatments != null ? data.Treatments.Count() > 0 : false)
                            data.TreatmentEntry = data.Treatments.First().Id;
                    }
                    data.Therapists = _therapistRepository.GetAll();
                    return View(data);
                }

                InfoModel infoModel = new InfoModel();
                infoModel.Title = "Add Appointment - Error";
                infoModel.Description = $"An error occurred while trying to edit the appointment for the patient";
                infoModel.Description += "\n, please contact the server admins to resolve this issue!";
                infoModel.HasImportantData = false;

                infoModel.TableEntries.Add("Reason: ", "The patient data could not be retrieved.");

                infoModel.ReturnAction = "Index";
                infoModel.ReturnController = "Appointment";

                HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                return RedirectToAction("Index", "Info", infoModel);
            }
            else
            {
                Patient patient = _patientRepository.Get(HttpContext.Session.GetObject<int>("UserDataId"));
                if (patient != null)
                {
                    if (ModelState.IsValid)
                    {
                        PatientFile patientFile = _patientFileRepository.Get(patient.PatientNumber);
                        if (patientFile != null)
                        {
                            Appointment app = _appointmentRepository.Get(data.Id);
                            Therapist therapist = _therapistRepository.Get(app.TherapistId.HasValue ? app.TherapistId.Value : -1);
                            if (therapist != null)
                            {
                                if (data.AppointmentDateTime.Value.IsBetween(therapist.AvailableFrom.Value, therapist.AvailableTo.Value))
                                {
                                    IEnumerable<Appointment> appointments = _appointmentRepository.GetByPatientNumber(patient.PatientNumber);
                                    int count = appointments.Where(x => x.AppointmentDateTime.Value.IsInSameWeek(data.AppointmentDateTime.Value)).Count();

                                    TreatmentPlan treatmentPlan = _treatmentPlanRepository.Get(patient.PatientNumber);
                                    if (data.AppointmentDateTime.Value > treatmentPlan.StartDate.Value)
                                    {
                                        if (data.AppointmentDateTime.Value < treatmentPlan.EndDate.Value)
                                        {
                                            data.TherapistId = therapist.Id;
                                            Appointment appointment = data.ForgeAppointment(_therapistRepository, _patientRepository, _treatmentRepository);
                                            (bool, Exception) result = _appointmentRepository.Update(appointment);
                                            if (result.Item1)
                                                return RedirectToAction("Index", "Appointment", new { pn = patient.PatientNumber });
                                        }
                                        else
                                            ModelState.AddModelError("AppointmentDateTime", "The Date/Time is higher than the End Date of the Treatment");
                                    }
                                    else
                                        ModelState.AddModelError("AppointmentDateTime", "The Date/Time is lower than the Start Date of the Treatment");
                                }
                                else
                                    ModelState.AddModelError("AppointmentDateTime", "Your therapist is not available in this timeframe");
                            }
                        }
                        else
                            return RedirectToAction("Index", "Appointment", new { pn = patient.PatientNumber });
                    }

                    data.Treatments = new List<Treatment>();
                    if (patient.PatientFileId.HasValue)
                    {
                        data.Treatments = _treatmentRepository.GetAllFromPatientFile(patient.PatientFileId.Value);
                        if (data.Treatments != null ? data.Treatments.Count() > 0 : false)
                            data.TreatmentEntry = data.Treatments.First().Id;
                    }
                    data.Therapists = _therapistRepository.GetAll();
                    return View(data);
                }

                InfoModel infoModel = new InfoModel();
                infoModel.Title = "Add Appointment - Error";
                infoModel.Description = $"An error occurred while trying to edit the appointment";
                infoModel.Description += "\n, please contact the server admins to resolve this issue.";
                infoModel.HasImportantData = false;

                infoModel.TableEntries.Add("Reason: ", "The patient data could not be retrieved.");

                infoModel.ReturnAction = "Index";
                infoModel.ReturnController = "Appointment";

                HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                return RedirectToAction("Index", "Info", infoModel);
            }
        }
    }
}